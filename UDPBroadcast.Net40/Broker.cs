using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using Anotar.CommonLogging;

// ReSharper disable UnusedParameter.Local
// ReSharper disable ExceptionNotDocumentedOptional
// ReSharper disable CatchAllClause
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace UDPBroadcast
{
  public partial class Broker : IDisposable
  {
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isDisposed;

    /// <exception cref="ArgumentNullException"><paramref name="messageSerializer" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="messageBodySerializer" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="messageFactory" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="pathFactory" /> is <see langword="null" />.</exception>
    public Broker(int port,
                  IMessageSerializer messageSerializer,
                  IMessageBodySerializer messageBodySerializer,
                  IMessageFactory messageFactory,
                  IPathFactory pathFactory)
    {
      if (messageSerializer == null)
      {
        throw new ArgumentNullException(nameof(messageSerializer));
      }
      if (messageBodySerializer == null)
      {
        throw new ArgumentNullException(nameof(messageBodySerializer));
      }
      if (messageFactory == null)
      {
        throw new ArgumentNullException(nameof(messageFactory));
      }
      if (pathFactory == null)
      {
        throw new ArgumentNullException(nameof(pathFactory));
      }

      this.Port = port;
      this.MessageSerializer = messageSerializer;
      this.MessageBodySerializer = messageBodySerializer;
      this.MessageFactory = messageFactory;
      this.PathFactory = pathFactory;

      this.ID = Guid.NewGuid();
    }

    public Guid ID { get; }
    private IMessageBodySerializer MessageBodySerializer { get; }
    private IMessageFactory MessageFactory { get; }
    private IMessageSerializer MessageSerializer { get; }
    private IPathFactory PathFactory { get; }
    private int Port { get; }
    private ConcurrentDictionary<string, ICollection<IMessageObserver>> TypeBasedObservers { get; } = new ConcurrentDictionary<string, ICollection<IMessageObserver>>();

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Receive(CancellationToken cancellationToken)
    {
      var ipEndPoint = new IPEndPoint(IPAddress.Any,
                                      this.Port);
      using (var udpClient = new UdpClient(this.Port))
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          byte[] buffer;
          try
          {
            buffer = udpClient.Receive(ref ipEndPoint);
          }
          catch (SocketException ex)
          {
            LogTo.ErrorException("some socket excpetion occured while receiving UDP packages", // Not L10N
                                 ex);
            continue;
          }
          catch (ObjectDisposedException)
          {
            break;
          }
          catch (Exception ex)
          {
            LogTo.ErrorException("some exception occured during receiving UDP packages", // Not L10N
                                 ex);
            continue;
          }

          IMessage message;
          try
          {
            message = this.MessageSerializer.Deserialize(buffer);
          }
          catch (Exception ex)
          {
            LogTo.ErrorException("could not deserialize message", // Not L10N
                                 ex);
            message = null;
          }

          if (message == null)
          {
            LogTo.Warn("received empty message"); // Not L10N
            continue;
          }
          if (message.Path == null)
          {
            LogTo.Warn($"received a message with an empty {nameof(message.Path)}");
            continue;
          }

          ICollection<IMessageObserver> observers;
          if (!this.TypeBasedObservers.TryGetValue(message.Path,
                                                   out observers))
          {
            return;
          }

          foreach (var observer in observers)
          {
            observer.OnNext(message);
          }
        }
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="obj" /> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during serialization.</exception>
    /// <exception cref="Exception">A generic exception occured during the creation of an <see cref="IMessage" /> instance.</exception>
    /// <exception cref="SocketException">
    ///   An error occurred when accessing the socket. See the Remarks section for more
    ///   information.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <see cref="Port" /> is less than
    ///   <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <see cref="Port" /> is greater than
    ///   <see cref="F:System.Net.IPEndPoint.MaxPort" />.
    /// </exception>
    public void Publish(object obj)
    {
      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      var body = this.MessageBodySerializer.Serialize(obj);
      var bodyType = obj.GetType();
      var path = this.PathFactory.GetPath(bodyType);

      var message = this.MessageFactory.Create();
      if (message == null)
      {
        LogTo.Error($"{nameof(message)} was null");
        return;
      }

      message.BrokerID = this.ID;
      message.Body = body;
      message.Path = path;

      this.Publish(message);
    }

    /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during serialization.</exception>
    /// <exception cref="Exception">A generic error has occurred during serialization.</exception>
    /// <exception cref="SocketException">
    ///   An error occurred when accessing the socket. See the Remarks section for more
    ///   information.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <see cref="Port" /> is less than
    ///   <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <see cref="Port" /> is greater than
    ///   <see cref="F:System.Net.IPEndPoint.MaxPort" />.
    /// </exception>
    public void Publish(IMessage message)
    {
      if (message == null)
      {
        throw new ArgumentNullException(nameof(message));
      }

      var dgram = this.MessageSerializer.Serialize(message);
      if (dgram == null)
      {
        LogTo.Error($"{nameof(dgram)} was null");
        return;
      }

      using (var udpClient = new UdpClient())
      {
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,
                                         SocketOptionName.Broadcast,
                                         1);
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,
                                         SocketOptionName.DontRoute,
                                         1);

        var ipEndPoint = new IPEndPoint(IPAddress.Broadcast,
                                        this.Port);

        udpClient.Send(dgram,
                       dgram.Count(),
                       ipEndPoint);
      }
    }

    /// <exception cref="ArgumentNullException"><paramref name="messageObserver" /> is <see langword="null" />.</exception>
    /// <exception cref="OverflowException">The dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
    public void Subscribe(IMessageObserver messageObserver)
    {
      if (messageObserver == null)
      {
        throw new ArgumentNullException(nameof(messageObserver));
      }

      var bodyType = messageObserver.GetBodyType();
      var path = this.PathFactory.GetPath(bodyType);

      var observers = this.TypeBasedObservers.GetOrAdd(path,
                                                       key => new LinkedList<IMessageObserver>());

      observers.Add(messageObserver);
    }

#if NET40 || NET46
    /// <exception cref="AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
#endif
    public void Start()
    {
      this.Stop();

      var cancellationTokenSource = new CancellationTokenSource();
      var cancellationToken = cancellationTokenSource.Token;

      this._cancellationTokenSource = cancellationTokenSource;

      this.Start(cancellationToken);
    }

    partial void Start(CancellationToken cancellationToken);

#if NET40 || NET46
    /// <exception cref="AggregateException">An aggregate exception containing all the exceptions thrown by the registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.</exception>
#endif
    public void Stop()
    {
      this._cancellationTokenSource?.Cancel();
      this._cancellationTokenSource = null;
    }

    ~Broker()
    {
      this.Dispose(false);
    }

    private void Dispose(bool disposing)
    {
      if (this._isDisposed)
      {
        return;
      }
      this._isDisposed = true;

      try
      {
        this._cancellationTokenSource?.Dispose();
        this._cancellationTokenSource = null;
      }
      catch (Exception exception)
      {
        LogTo.ErrorException("could not dispose CancellationTokenSource", // Not L10N
                             exception);
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Anotar.CommonLogging;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CatchAllClause
// ReSharper disable UnusedMember.Global

namespace UDPBroadcast
{
  public partial class Broker : IDisposable
  {
    private const int Started = 1;
    private const int NotStarted = 0;

    private bool _isDisposed;
    private int _isStarted = Broker.NotStarted;

    public Broker(int port)
    {
      this.Port = port;
      this.ID = Guid.NewGuid();

      this.TypeBasedObservers = new Dictionary<string, ICollection<IMessageObserver>>();
      this.CancellationTokenSource = new CancellationTokenSource();

      this.PathFactory = type => type.FullName;
      this.ObserverFactory = () => new LinkedList<IMessageObserver>();
      this.DeserializeMessageFn = buffer =>
                                  {
                                    // ReSharper disable ExceptionNotDocumentedOptional
                                    if (buffer == null)
                                    {
                                      throw new ArgumentNullException(nameof(buffer));
                                    }

                                    using (var memoryStream = new MemoryStream(buffer))
                                    {
                                      var binaryFormatter = new BinaryFormatter();
                                      var obj = binaryFormatter.Deserialize(memoryStream);
                                      var message = (IMessage) obj;

                                      return message;
                                    }
                                    // ReSharper restore ExceptionNotDocumentedOptional
                                  };
      this.MessageFactory = () => new Message();
      this.SerializeMessageFn = message =>
                                {
                                  // ReSharper disable ExceptionNotDocumentedOptional
                                  if (message == null)
                                  {
                                    throw new ArgumentNullException(nameof(message));
                                  }

                                  byte[] buffer;

                                  var binaryFormatter = new BinaryFormatter();
                                  using (var memoryStream = new MemoryStream())
                                  {
                                    binaryFormatter.Serialize(memoryStream,
                                                              message);

                                    buffer = memoryStream.ToArray();
                                  }

                                  return buffer;
                                  // ReSharper restore ExceptionNotDocumentedOptional
                                };
    }

    private CancellationTokenSource CancellationTokenSource { get; }
    public Func<byte[], IMessage> DeserializeMessageFn { get; set; }
    public Guid ID { get; }
    public Func<IMessage> MessageFactory { get; set; }
    public Func<ICollection<IMessageObserver>> ObserverFactory { get; set; }
    public Func<Type, string> PathFactory { get; set; }
    private int Port { get; }
    public Func<IMessage, byte[]> SerializeMessageFn { get; set; }
    private IDictionary<string, ICollection<IMessageObserver>> TypeBasedObservers { get; }

    public void Dispose()
    {
      this.Dispose(true);
      // ReSharper disable ExceptionNotDocumentedOptional
      GC.SuppressFinalize(this);
      // ReSharper restore ExceptionNotDocumentedOptional
    }

#if NET40 || NET46
    /// <exception cref="ObjectDisposedException">The token source has been disposed.</exception>
#endif
    public void Start()
    {
      // ReSharper disable ExceptionNotDocumented
      if (Interlocked.CompareExchange(ref this._isStarted,
                                      Broker.Started,
                                      Broker.NotStarted) != Broker.NotStarted)
      {
        return;
      }
      // ReSharper restore ExceptionNotDocumented

      var cancellationToken = this.CancellationTokenSource.Token;

      this.Start(cancellationToken);
    }

    partial void Start(CancellationToken cancellationToken);

    ~Broker()
    {
      this.Dispose(false);
    }

    // ReSharper disable UnusedParameter.Local
    private void Dispose(bool disposing)
    {
      if (this._isDisposed)
      {
        return;
      }
      this._isDisposed = true;

      try
      {
        this.CancellationTokenSource.Dispose();
      }
      catch (Exception exception)
      {
        LogTo.ErrorException("could not dispose CancellationTokenSource", // Not L10N
                             exception);
      }
    }

    // ReSharper restore UnusedParameter.Local

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
            LogTo.WarnException("some socket excpetion occured while receiving UDP packages", // Not L10N
                                ex);
            continue;
          }
          catch (ObjectDisposedException)
          {
            break;
          }
          catch (Exception ex)
          {
            LogTo.WarnException("some exception occured during receiving UDP packages", // Not L10N
                                ex);
            continue;
          }

          var deserializMessageFn = this.DeserializeMessageFn;
          if (deserializMessageFn == null)
          {
            LogTo.Error("{0} is null", // Not L10N
                        nameof(this.DeserializeMessageFn));
            continue;
          }

          IMessage message;
          try
          {
            message = deserializMessageFn.Invoke(buffer);
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
            try
            {
              observer.OnNext(message);
            }
            catch (Exception exNext)
            {
              try
              {
                observer.OnError(exNext);
              }
              catch (Exception exError)
              {
                LogTo.ErrorException("failed to invoke {0} for message at {1}", // Not L10N
                                     exError,
                                     nameof(IObserver<IMessage>.OnError),
                                     message.Path);
              }

              LogTo.ErrorException("could not handle message for {0}", // Not L10N
                                   exNext,
                                   message.Path);
            }
          }
        }
      }
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="SocketException">
    ///   An error occurred when accessing the socket. See the Remarks section for more
    ///   information.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <see cref="Port" /> is less than
    ///   <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <see cref="Port" /> is greater than
    ///   <see cref="F:System.Net.IPEndPoint.MaxPort" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">If <see cref="MessageFactory" /> is null.</exception>
    /// <exception cref="InvalidOperationException">If <see cref="PathFactory" /> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
#if NET45 || NET46
    /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
#endif
    public void Publish(object obj)
    {
      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      var messageFactory = this.MessageFactory;
      if (messageFactory == null)
      {
        throw new InvalidOperationException($"{nameof(this.MessageFactory)} is null");
      }

      var pathFactory = this.PathFactory;
      if (pathFactory == null)
      {
        throw new InvalidOperationException($"{nameof(this.PathFactory)} is null");
      }

      var bodyType = obj.GetType();
      var path = pathFactory.Invoke(bodyType);

      var message = messageFactory.Invoke();
      message.SetBrokerID(this.ID);
      message.SetInstance(obj);
      message.SetPath(path);

      this.Publish(message);
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="SocketException">
    ///   An error occurred when accessing the socket. See the Remarks section for more
    ///   information.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <see cref="Port" /> is less than
    ///   <see cref="F:System.Net.IPEndPoint.MinPort" />.-or- <see cref="Port" /> is greater than
    ///   <see cref="F:System.Net.IPEndPoint.MaxPort" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">If <see cref="SerializeMessageFn" /> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null" />.</exception>
#if NET45 || NET46
    /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
#endif
    public void Publish(IMessage message)
    {
      if (message == null)
      {
        throw new ArgumentNullException(nameof(message));
      }

      var serializeMessageFn = this.SerializeMessageFn;
      if (serializeMessageFn == null)
      {
        throw new InvalidOperationException($"{nameof(this.SerializeMessageFn)} is null");
      }

      var dgram = serializeMessageFn.Invoke(message);

      using (var udpClient = new UdpClient())
      {
        // ReSharper disable ExceptionNotDocumentedOptional
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,
                                         SocketOptionName.Broadcast,
                                         1);
        // ReSharper restore ExceptionNotDocumentedOptional
        // ReSharper disable ExceptionNotDocumentedOptional
        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,
                                         SocketOptionName.DontRoute,
                                         1);
        // ReSharper restore ExceptionNotDocumentedOptional

        // ReSharper disable ExceptionNotDocumentedOptional
        var ipEndPoint = new IPEndPoint(IPAddress.Broadcast,
                                        this.Port);
        // ReSharper restore ExceptionNotDocumentedOptional

        // ReSharper disable ExceptionNotDocumentedOptional
        udpClient.Send(dgram,
                       dgram.Length,
                       ipEndPoint);
        // ReSharper restore ExceptionNotDocumentedOptional
      }
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="InvalidOperationException">If no observer collection could be created.</exception>
    /// <exception cref="InvalidOperationException">If <see cref="PathFactory" /> is null.</exception>
    /// <exception cref="InvalidOperationException">If <see cref="ObserverFactory" /> is null.</exception>
    /// <exception cref="InvalidOperationException">If the observer collection is read-only.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="messageObserver"/> is <see langword="null" />.</exception>
    public void Subscribe(IMessageObserver messageObserver)
    {
      if (messageObserver == null)
      {
        throw new ArgumentNullException(nameof(messageObserver));
      }

      var pathFactory = this.PathFactory;
      if (pathFactory == null)
      {
        throw new InvalidOperationException($"{nameof(this.PathFactory)} is null");
      }

      var bodyType = messageObserver.GetBodyType();
      var path = pathFactory.Invoke(bodyType);
      if (path == null)
      {
        throw new InvalidOperationException($"Path for {bodyType} was null");
      }

      ICollection<IMessageObserver> observers;
      // ReSharper disable ExceptionNotDocumentedOptional
      if (!this.TypeBasedObservers.TryGetValue(path,
                                               out observers))
      {
        var observerFactory = this.ObserverFactory;
        if (observerFactory == null)
        {
          throw new InvalidOperationException($"{nameof(this.ObserverFactory)} is null");
        }

        observers = observerFactory.Invoke();
        if (observers == null)
        {
          throw new InvalidOperationException("The created observer collection is null"); // Not L10N
        }
        if (observers.IsReadOnly)
        {
          throw new InvalidOperationException("The observer collection is read-only"); // Not L10N
        }

        this.TypeBasedObservers.Add(path,
                                    observers);
      }
      // ReSharper restore ExceptionNotDocumentedOptional

      // ReSharper disable ExceptionNotDocumentedOptional
      observers.Add(messageObserver);
      // ReSharper restore ExceptionNotDocumentedOptional
    }
  }
}

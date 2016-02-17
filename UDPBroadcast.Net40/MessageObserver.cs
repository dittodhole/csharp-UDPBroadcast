// ReSharper disable RedundantUsingDirective
using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using Anotar.CommonLogging;

// ReSharper disable CatchAllClause
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace UDPBroadcast
{
  public class MessageObserver<T> : IMessageObserver<T>
    where T : class
  {
    public MessageObserver(Guid brokerID,
                           IMessageBodySerializer messageBodySerializer)
    {
      this.BrokerID = brokerID;
      this.MessageBodySerializer = messageBodySerializer;
    }

    private Guid BrokerID { get; }
    public Action InterceptCompleted { get; set; }
    public Action<Exception> InterceptOnError { get; set; }
    public Action<T> InterceptOnNext { get; set; }
    public bool InterceptRemoteMessagesOnly { get; set; } = true;
    private IMessageBodySerializer MessageBodySerializer { get; }

    public void OnNext(IMessage value)
    {
      if (value == null)
      {
        return;
      }

      if (this.InterceptRemoteMessagesOnly)
      {
        if (this.BrokerID == value.BrokerID)
        {
          return;
        }
      }

      object obj;
      try
      {
        obj = this.MessageBodySerializer.Deserialize(value.Body);
      }
      catch (SerializationException serializationException)
      {
        LogTo.ErrorException($"could not deserialize the {nameof(IMessage.Body)} of {nameof(value)}",
                             serializationException);
        return;
      }
      catch (ArgumentNullException argumentNullException)
      {
        LogTo.ErrorException($"the {nameof(IMessage.Body)} property of {nameof(value)} was null",
                             argumentNullException);
        return;
      }
      catch (Exception exception)
      {
        LogTo.ErrorException($"a generic error occurred during {nameof(this.OnNext)}",
                             exception);
        return;
      }
      var instance = obj as T;

      try
      {
        this.InterceptOnNext?.Invoke(instance);
      }
      catch (Exception exception)
      {
        LogTo.ErrorException($"error during calling {nameof(this.InterceptOnNext)}",
                             exception);

        this.OnError(exception);
      }
    }

    public void OnError(Exception error)
    {
      try
      {
        this.InterceptOnError?.Invoke(error);
      }
      catch (Exception exception)
      {
        LogTo.ErrorException($"exception during calling {nameof(this.InterceptOnError)}",
                             exception);
      }
    }

    public void OnCompleted()
    {
      try
      {
        this.InterceptCompleted?.Invoke();
      }
      catch (Exception exception)
      {
        LogTo.ErrorException($"exception during calling {nameof(this.InterceptCompleted)}",
                             exception);
      }
    }

    public Type GetBodyType()
    {
      return typeof (T);
    }
  }
}

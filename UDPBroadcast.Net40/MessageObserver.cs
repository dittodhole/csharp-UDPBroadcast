using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace UDPBroadcast
{
  public class MessageObserver<T> : IMessageObserver<T>
  {
    public MessageObserver(Guid brokerID)
    {
      this.BrokerID = brokerID;
      this.InterceptRemoteMessagesOnly = true;
    }

    private Guid BrokerID { get; }
    public Action InterceptCompleted { get; set; }
    public Action<Exception> InterceptOnError { get; set; }
    public Action<T> InterceptOnNext { get; set; }
    public bool InterceptRemoteMessagesOnly { get; set; }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
    public void OnNext(IMessage value)
    {
      if (value == null)
      {
        throw new ArgumentNullException(nameof(value));
      }

      if (this.InterceptRemoteMessagesOnly)
      {
        if (this.BrokerID == value.BrokerID)
        {
          return;
        }
      }

      var obj = value.GetInstance();
      var instance = (T) obj;

      this.InterceptOnNext?.Invoke(instance);
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public void OnError(Exception error)
    {
      this.InterceptOnError?.Invoke(error);
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    public void OnCompleted()
    {
      this.InterceptCompleted?.Invoke();
    }

    public Type GetBodyType()
    {
      return typeof (T);
    }
  }
}

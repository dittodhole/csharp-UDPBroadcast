using System;

namespace UDPBroadcast
{
  public interface IMessageObserver : IObserver<IMessage>
  {
    Type GetBodyType();
  }

  public interface IMessageObserver<T> : IMessageObserver
  {
  }
}

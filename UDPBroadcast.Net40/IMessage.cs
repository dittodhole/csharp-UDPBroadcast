using System;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace UDPBroadcast
{
  public interface IMessage
  {
    byte[] Body { get; }
    Guid BrokerID { get; }
    string Path { get; }
    void SetInstance(object obj);
    object GetInstance();
  }
}

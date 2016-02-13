using System;

// ReSharper disable UnusedMember.Global

namespace UDPBroadcast
{
  public interface IMessage
  {
    byte[] Body { get; }
    Guid BrokerID { get; }
    string Path { get; }
  }
}

using System;

namespace UDPBroadcast
{
  public interface IMessage
  {
    byte[] Body { get; set; }
    Guid BrokerID { get; set; }
    string Path { get; set;  }
  }
}

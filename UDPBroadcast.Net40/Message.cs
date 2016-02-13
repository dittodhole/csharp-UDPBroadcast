using System;

namespace UDPBroadcast
{
  public class Message : IMessage
  {
    public string Path { get; set; }
    public byte[] Body { get; set; }
    public Guid BrokerID { get; set; }
  }
}

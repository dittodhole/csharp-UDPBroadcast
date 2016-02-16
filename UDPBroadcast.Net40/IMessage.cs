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

    /// <exception cref="Exception">A generic exception may occur during <see cref="SetInstance"/>.</exception>
    void SetInstance(object obj);

    void SetPath(string path);
    void SetBrokerID(Guid brokerID);

    /// <exception cref="Exception">A generic exception may occur during <see cref="GetInstance"/>.</exception>
    object GetInstance();
  }
}

﻿using System;
using System.Runtime.Serialization;

namespace UDPBroadcast
{
  public interface IMessageSerializer
  {
    /// <exception cref="Exception">A generic error has occurred during deserialization.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during deserialization.</exception>
    IMessage Deserialize(byte[] buffer);

    /// <exception cref="Exception">A generic error has occurred during serialization.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message" /> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during serialization.</exception>
    byte[] Serialize(IMessage message);
  }
}

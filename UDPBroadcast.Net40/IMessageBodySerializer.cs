using System;
using System.Runtime.Serialization;

namespace UDPBroadcast
{
  public interface IMessageBodySerializer
  {
    /// <exception cref="Exception">A generic error has occurred during deserialization.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during deserialization.</exception>
    object Deserialize(byte[] buffer);

    /// <exception cref="Exception">A generic error has occurred during serialization.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during serialization.</exception>
    byte[] Serialize(object obj);
  }
}

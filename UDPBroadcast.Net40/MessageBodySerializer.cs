using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

// ReSharper disable ExceptionNotThrown
// ReSharper disable ExceptionNotDocumented

namespace UDPBroadcast
{
  public class MessageBodySerializer : IMessageBodySerializer
  {
    /// <exception cref="Exception">A generic error has occurred during deserialization.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during deserialization.</exception>
    public object Deserialize(byte[] buffer)
    {
      if (buffer == null)
      {
        throw new ArgumentNullException(nameof(buffer));
      }

      using (var memoryStream = new MemoryStream(buffer))
      {
        var binaryFormatter = new BinaryFormatter();
        var obj = binaryFormatter.Deserialize(memoryStream);

        return obj;
      }
    }

    /// <exception cref="Exception">A generic error has occurred during serialization.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during serialization.</exception>
    public byte[] Serialize(object obj)
    {
      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      var binaryFormatter = new BinaryFormatter();
      using (var memoryStream = new MemoryStream())
      {
        binaryFormatter.Serialize(memoryStream,
                                  obj);
        var body = memoryStream.ToArray();

        return body;
      }
    }
  }
}

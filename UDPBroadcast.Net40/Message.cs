using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// ReSharper disable MemberCanBePrivate.Global

namespace UDPBroadcast
{
  [Serializable]
  public class Message : IMessage
  {
    static Message()
    {
      Message.SerializeBodyFn = obj =>
                                {
                                  // ReSharper disable ExceptionNotDocumentedOptional
                                  if (obj == null)
                                  {
                                    throw new ArgumentNullException(nameof(obj));
                                  }

                                  byte[] buffer;

                                  var binaryFormatter = new BinaryFormatter();
                                  using (var memoryStream = new MemoryStream())
                                  {
                                    binaryFormatter.Serialize(memoryStream,
                                                              obj);
                                    buffer = memoryStream.ToArray();
                                  }

                                  return buffer;
                                  // ReSharper restore ExceptionNotDocumentedOptional
                                };
      Message.DeserializeBodyFn = buffer =>
                                  {
                                    // ReSharper disable ExceptionNotDocumentedOptional
                                    if (buffer == null)
                                    {
                                      throw new ArgumentNullException(nameof(buffer));
                                    }

                                    object obj;
                                    using (var memoryStream = new MemoryStream(buffer))
                                    {
                                      var binaryFormatter = new BinaryFormatter();
                                      obj = binaryFormatter.Deserialize(memoryStream);
                                    }

                                    return obj;
                                    // ReSharper restore ExceptionNotDocumentedOptional
                                  };
    }

    public static Func<byte[], object> DeserializeBodyFn { get; set; }
    public static Func<object, byte[]> SerializeBodyFn { get; set; }

    public string Path { get; set; }
    public byte[] Body { get; set; }
    public Guid BrokerID { get; set; }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="InvalidOperationException">If <see cref="SerializeBodyFn" /> is null.</exception>
    public void SetInstance(object obj)
    {
      var serializeBodyFn = Message.SerializeBodyFn;
      if (serializeBodyFn == null)
      {
        throw new InvalidOperationException($"{nameof(Message.SerializeBodyFn)} is null");
      }
      var body = serializeBodyFn.Invoke(obj);

      this.Body = body;
    }

    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
    /// <exception cref="InvalidOperationException">If <see cref="DeserializeBodyFn" /> is null.</exception>
    public object GetInstance()
    {
      var deserializeBodyFn = Message.DeserializeBodyFn;
      if (deserializeBodyFn == null)
      {
        throw new InvalidOperationException($"{nameof(Message.DeserializeBodyFn)} is null");
      }

      var obj = deserializeBodyFn.Invoke(this.Body);
      return obj;
    }
  }
}

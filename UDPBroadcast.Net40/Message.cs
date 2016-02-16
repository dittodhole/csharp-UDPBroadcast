using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

// ReSharper disable MemberCanBePrivate.Global

namespace UDPBroadcast
{
  [Serializable]
  public class Message : IMessage
  {
    public string Path { get; set; }
    public byte[] Body { get; set; }
    public Guid BrokerID { get; set; }

    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    /// <exception cref="SerializationException">An error has occurred during serialization, such as if an object in the <paramref name="graph" /> parameter is not marked as serializable. </exception>
    /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
    public void SetInstance(object obj)
    {
      if (obj == null)
      {
        throw new ArgumentNullException(nameof(obj));
      }

      byte[] body;

      var binaryFormatter = new BinaryFormatter();
      using (var memoryStream = new MemoryStream())
      {
        binaryFormatter.Serialize(memoryStream,
                                  obj);
        body = memoryStream.ToArray();
      }

      this.Body = body;
    }

    public void SetPath(string path)
    {
      this.Path = path;
    }

    public void SetBrokerID(Guid brokerID)
    {
      this.BrokerID = brokerID;
    }

    /// <exception cref="InvalidOperationException">If <see cref="Body"/> is null.</exception>
    /// <exception cref="SerializationException">The <paramref name="serializationStream" /> supports seeking, but its length is 0. -or-The target type is a <see cref="T:System.Decimal" />, but the value is out of range of the <see cref="T:System.Decimal" /> type.</exception>
    /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
    public object GetInstance()
    {
      var body = this.Body;
      if (body == null)
      {
        throw new InvalidOperationException($"{nameof(this.Body)} is null");
      }

      object obj;
      using (var memoryStream = new MemoryStream(body))
      {
        var binaryFormatter = new BinaryFormatter();
        obj = binaryFormatter.Deserialize(memoryStream);
      }

      return obj;
    }
  }
}

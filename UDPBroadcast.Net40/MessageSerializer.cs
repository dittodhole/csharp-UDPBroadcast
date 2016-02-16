using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;

// ReSharper disable UnusedMember.Global

namespace UDPBroadcast
{
    public class MessageSerializer : IMessageSerializer
    {
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="SerializationException">
        ///     The <paramref name="serializationStream"/> supports seeking, but its length is
        ///     0. -or-The target type is a <see cref="T:System.Decimal"/>, but the value is out of range of the
        ///     <see cref="T:System.Decimal"/> type.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        public IMessage Deserialize(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            using (var memoryStream = new MemoryStream(buffer))
            {
                var binaryFormatter = new BinaryFormatter();
                var obj = binaryFormatter.Deserialize(memoryStream);
                var message = (IMessage) obj;

                return message;
            }
        }

        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
        /// <exception cref="SerializationException">
        ///     An error has occurred during serialization, such as if an object in the
        ///     <paramref name="graph"/> parameter is not marked as serializable.
        /// </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        public byte[] Serialize(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            byte[] buffer;

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream,
                                          message);

                buffer = memoryStream.ToArray();
            }

            return buffer;
        }
    }
}

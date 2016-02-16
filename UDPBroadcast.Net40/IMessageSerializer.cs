using System;

namespace UDPBroadcast
{
    public interface IMessageSerializer
    {
        /// <exception cref="Exception">A generic exception may occur during <see cref="Deserialize"/>.</exception>
        IMessage Deserialize(byte[] buffer);

        /// <exception cref="Exception">A generic exception may occur during <see cref="Serialize"/>.</exception>
        byte[] Serialize(IMessage message);
    }
}

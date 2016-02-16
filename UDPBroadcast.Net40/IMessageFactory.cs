using System;

namespace UDPBroadcast
{
    public interface IMessageFactory
    {
        /// <exception cref="Exception">A generic exception may occur during <see cref="Create"/>.</exception>
        IMessage Create();
    }
}

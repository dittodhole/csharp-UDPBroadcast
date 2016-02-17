using System;

namespace UDPBroadcast
{
    public interface IMessageFactory
    {
        /// <exception cref="Exception">A generic exception occured during <see cref="Create"/>.</exception>
        IMessage Create();
    }
}

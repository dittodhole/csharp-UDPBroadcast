using System;

namespace UDPBroadcast
{
  public interface IPathFactory
  {
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null" />.</exception>
    string GetPath(Type type);
  }
}

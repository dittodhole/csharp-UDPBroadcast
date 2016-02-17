using System;

namespace UDPBroadcast
{
  public class PathFactory : IPathFactory
  {
    /// <exception cref="ArgumentNullException"><paramref name="type" /> is <see langword="null" />.</exception>
    public string GetPath(Type type)
    {
      if (type == null)
      {
        throw new ArgumentNullException(nameof(type));
      }

      return type.FullName;
    }
  }
}

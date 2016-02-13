using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace UDPBroadcast
{
  public partial class Broker
  {
    /// <exception cref="ArgumentNullException"><paramref name="cancellationToken" /> is <see langword="null" />.</exception>
    partial void Start(CancellationToken cancellationToken)
    {
      if (cancellationToken == null)
      {
        throw new ArgumentNullException(nameof(cancellationToken));
      }

      Task.Run(() => this.Receive(),
               cancellationToken);
    }
  }
}

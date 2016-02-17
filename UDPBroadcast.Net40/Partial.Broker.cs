using System.Threading;
using System.Threading.Tasks;

namespace UDPBroadcast
{
  public partial class Broker
  {
    partial void Start(CancellationToken cancellationToken)
    {
      TaskEx.Run(() => this.Receive(cancellationToken),
                 cancellationToken);
    }
  }
}

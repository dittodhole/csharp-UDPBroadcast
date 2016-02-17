using System.Threading;
using System.Threading.Tasks;

namespace UDPBroadcast
{
  public partial class Broker
  {
    partial void Start(CancellationToken cancellationToken)
    {
      Task.Run(() => this.Receive(cancellationToken),
               cancellationToken);
    }
  }
}

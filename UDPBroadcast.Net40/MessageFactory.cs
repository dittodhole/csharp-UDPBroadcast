namespace UDPBroadcast
{
  public class MessageFactory : IMessageFactory
  {
    public IMessage Create()
    {
      var message = new Message();

      return message;
    }
  }
}

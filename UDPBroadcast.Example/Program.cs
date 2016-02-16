using System;

namespace UDPBroadcast.Example
{
  class Program
  {
    static void Main(string[] args)
    {
      var messageSerializer = new MessageSerializer();
      var messageFactory = new MessageFactory();
      var broker = new Broker(1337,
                              messageSerializer,
                              messageFactory);
      var messageObserver = new MessageObserver<Foo>(broker.ID)
                            {
                              InterceptRemoteMessagesOnly = false,
                              InterceptOnNext = foo =>
                                                {
                                                  // TODO what to do next ...
                                                }
                            };
      broker.Subscribe(messageObserver);
      broker.Start();

      broker.Publish(new Foo
                     {
                       Bar = "hello"
                     });

      Console.ReadLine();
    }
  }
}

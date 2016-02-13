using System;

namespace UDPBroadcast.Example
{
  class Program
  {
    static void Main(string[] args)
    {
      var broker = new Broker(1337);
      var messageObserver = new MessageObserver(broker.ID)
                            {
                              InterceptRemoteMessagesOnly = false,
                              InterceptOnNext = message =>
                                                {
                                                  var foo = broker.GetInstance<Foo>(message);
                                                  // TODO what to do next ...
                                                }
                            };
      broker.Subscribe<Foo>(messageObserver);
      broker.Start();

      {
        var foo = new Foo
                  {
                    Bar = "hello"
                  };
        broker.Publish(foo);
      }

      Console.ReadLine();
    }
  }
}

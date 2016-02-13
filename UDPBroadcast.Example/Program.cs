using System;

namespace UDPBroadcast.Example
{
  class Program
  {
    static void Main(string[] args)
    {
      var broker = new Broker(1337);
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

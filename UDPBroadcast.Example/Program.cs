using System;

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedParameter.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace UDPBroadcast.Example
{
  class Program
  {
    static void Main(string[] args)
    {
      var messageSerializer = new MessageSerializer();
      var messageBodySerializer = new MessageBodySerializer();
      var messageFactory = new MessageFactory();
      var pathFactory = new PathFactory();
      var broker = new Broker(1337,
                              messageSerializer,
                              messageBodySerializer,
                              messageFactory,
                              pathFactory);
      var messageObserver = new MessageObserver<Foo>(broker.ID,
                                                     messageBodySerializer)
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
                       Bar = "hello" // Not L10N
                     });

      Console.ReadLine();
    }
  }
}

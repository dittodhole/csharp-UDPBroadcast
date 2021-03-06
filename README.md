# ![Icon](https://raw.githubusercontent.com/dittodhole/csharp-UDPBroadcast/master/Icons/package_icon.png) csharp-UDPBroadcast

This project's aim is to provide a simple interface to broadcast messages within the local network by using [UDP](https://en.wikipedia.org/wiki/User_Datagram_Protocol). Just in case you can't do any [Zeroconf](https://en.wikipedia.org/wiki/Zero-configuration_networking), [UPnP](https://en.wikipedia.org/wiki/Universal_Plug_and_Play), [PGM](https://en.wikipedia.org/wiki/Pragmatic_General_Multicast), or message queuing with publish semantics to register and publish services with zero to none configuration effort.

## Installing [![NuGet Status](http://img.shields.io/nuget/v/UDPBroadcast.svg?style=flat)](https://www.nuget.org/packages/UDPBroadcast/)

https://www.nuget.org/packages/UDPBroadcast/

    PM> Install-Package UDPBroadcast

## Usage

    using System;
    using UDPBroadcast;

    [Serializable] // is needed as we are using BinaryFormatter internally by default
    public sealed class Foo
    {
      public string Bar { get; set; }
    }
    
    var messageSerializer = new MessageSerializer();
    var messageBodySerializer = new MessageBodySerializer();
    var messageFactory = new MessageFactory();
    var pathFactory = new PathFactory();
    using (var broker = new Broker(1337,
                                   messageSerializer,
                                   messageBodySerializer,
                                   messageFactory,
                                   pathFactory))
    {
      var messageObserver = new MessageObserver<Foo>(broker.ID)
      {
        InterceptRemoteMessagesOnly = false,
        InterceptOnNext = foo =>
        {
          // yolo
        }
      };
      broker.Subscribe(messageObserver);
      broker.Start();
    
      broker.Publish(new Foo
      {
        Bar = "hello"
      });

      Console.ReadLine(); // or whatever mechanism you want to use to block in this example
    }

### Adapting serialization/creation ...

**IMessageFactory**
You can inject your own `IMessageFactory` implementation, to send eg `CustomMessage` instances instead to default `Message`.

**IMessageSerializer**
Instead of using the default `BinaryFormatter` as the serializer of `IMessage` instances, you can provide your own serialization mechanism.

**IMessageBodySerializer**
Instead of using the default `BinaryFormatter` as the serializer of `IMessage.Body`, you can provide your own serialization mechanism.

**IPathFactory**
If you ever come across the need to adapt routing from the default `Type.FullName` of the encapsulated instance, you can provider your own implementation.

## License

csharp-UDPBroadcast is published under [WTFNMFPLv3](http://andreas.niedermair.name/introducing-wtfnmfplv3).

## Spotify Playlist

While implementing this project, I listened to [this playlist](https://open.spotify.com/user/dittodhole/playlist/4iTsAO3Az90sdVJY4AX8di). If you want to make adaptions to the project, I strongly recommend listening to this playlist, as it gets you in the right mood.

## Icon

[paper plane](https://thenounproject.com/jum/uploads/?i=156217) by [Jems Mayor](https://thenounproject.com/jum/) from the Noun Project.
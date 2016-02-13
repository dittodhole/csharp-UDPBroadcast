# csharp-UDPBroadcast

This project's aim is to provide a simple interface to broadcast messages within the local network over [UDP](https://en.wikipedia.org/wiki/User_Datagram_Protocol). Just in case you can't do any [Zeroconf](https://en.wikipedia.org/wiki/Zero-configuration_networking), [UPnP](https://en.wikipedia.org/wiki/Universal_Plug_and_Play), [PGM](https://en.wikipedia.org/wiki/Pragmatic_General_Multicast), or message queuing with publish semantics to register and publish services with zero to none configuration effort.

## Installing

    PM> Install-Package UDPBroadcast

## Example

	using System;    
	using UDPBroadcast;
    
    [Serializable]
	public class Foo
    {
      public string Bar { get; set; }
    }
    
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

## Adapting serialization/routing/creation ...

Instead of implementing your own broker, the following hooks in the `Broker` class exist to adapt the serialization/routing/creation of `IMessage` instances:

- **MessageFactory** : `Func<IMessage>`
You want to create a `CustomMessage` class and use this instead of `Message`? This factory delegate can be used to customize the creation of an `IMessage` instance.
- **PathFactory**: `Func<Type, string>`
This delegate returns a unique path based on the `Type` argument to allow distinct routing of `IMessage` instances and their bodies.
- **SerializeMessageFn** : `Func<IMessage, byte[]>`
You can replace the default delegate to adapt the serialization of `IMessage` instances.
- **DeserializeMessageFn** : `Func<byte[], IMessage>`
Please don't forget to adapt this property when exchanging the serialization-mechanism. You need a counterpart to that in the end.

Additionally you one can also adapt the serialization of instances to the `Body` property of `Message` instances (which are created by default in `MessageFactory` of `Broker`) with the following hooks:

- **SerializeBodyFn** : `Func<object, byte[]>`
This delegate is responsible for serializing instances inside the `SetInstance` method.
- **DeserializeBodyFn** : `Func<byte[], object>`
The same that applies for `DeserializeMessageFn` of `Broker` also applies here: If you adapt the serialization-mechanism, don't forget about the way back. 
    

## License

csharp-UDPBroadcast is published under [WTFNMFPLv3](http://andreas.niedermair.name/introducing-wtfnmfplv3).

## Spotify Playlist

While implementing this project, I listened [this playlist](https://open.spotify.com/user/dittodhole/playlist/4iTsAO3Az90sdVJY4AX8di). If you want to make adaptions to the project, I strongly recommend listening to this playlist, as it gets you in the right mood.
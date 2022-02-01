# Osprey

Osprey is a .NET library for messaging and streaming across distributed systems intended for use on local area networks. It is designed to require zero configuration and be fully decentralised, meaning a large number of distributed services can interact with eachother and handle failures without intervention. For example, suppose a client application is receiving data through a stream from a service. If the server fails, the client can automatically locate an equivalent service and re-establish the stream.

Features:
* Decentralized service discovery
* Automatic load balancing
* ZeroMQ streams
  * Brokerless
  * Self-recovering
* HTTP messaging
* Simple and lightweight

Here is a server setting itself up with a ZeroMQ streaming endpoint:
```
using (Osprey.Default()) // Setup Osprey with defaults (Provides a JSON Serializer, etc.)
using (Osprey.Join("osprey.server")) // Join the osprey network with this node's service name being 'osprey.server'
using (var stream = new ZeroMQServer("my-stream-endpoint")) // Setup a streaming endpoint called 'my-stream-endpoint'
{
    Task.Run(() =>
    {
        var rnd = new Random();
        while (true)
        {
            // Create some data with a random number
            var data = new TestData()
            {
                Data1 = rnd.Next(1, 999)
            };

            // Publish the data to topic "A"
            stream.Publish("A", data);

            Thread.Sleep(100);
        }
    });
    Console.ReadKey();
}
```

And this is a client consuming the server's stream:
```
using (Osprey.Default())
using (Osprey.Join("osprey.client"))
using (var client = new ZeroMQClient("osprey.server", "my-stream-endpoint"))
{
    // Tell the client what to do when disconnected from the server
    client.OnDisconnected += () => Console.WriteLine("CLIENT HAS DISCONNECTED");

    // Tell the client what to do when connected/re-connected to the server
    client.OnConnected += () =>
    {
        Console.WriteLine("CLIENT HAS CONNECTED");
        client.Subscribe("A");
    };

    // Tell the client to deserialize messages from topic "A" into a TestData object and provide a handler.
    client.On<TestData>("A", msg =>
    {
        // Print the data that we have received from the server
        Console.WriteLine(msg.Data1);
    });

    // Start the connection to the server
    client.Connect();

    Console.ReadKey();
}
```
# Osprey

Osprey is a framework for messaging and streaming across distributed systems intended for use on local area networks.

Features:
* Zero-configuration service discovery
* Brokerless communication
* HTTP messaging
* Self-recovering streams

Here is a server setting itself up with a ZeroMQ streaming endpoint and HTTP server
```
using (Osprey.Default())
using (Osprey.Join("osprey.server"))
using (new ZeroMQServer("guava"))
using (new HttpServer<DefaultStartup<DefaultNancyBootstrapper>>("mango"))
{
    Console.ReadKey();
}
```

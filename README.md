# Osprey

Here is a server setting itself up with a TCP streaming endpoint and HTTP server
```
using (Osprey.Default())
using (Osprey.Join("osprey.server"))
using (new TcpServer("guava"))
using (new HttpServer<DefaultStartup<DefaultNancyBootstrapper>>("mango"))
{
    Console.ReadKey();
}
```

And here is a client using a HTTP endpoint on the server:

```
using (Osprey.Default())
using (Osprey.Join("osprey.client"))
{
    while (true)
    {
        try
        {
            var response = await Osprey.Locate("osprey.client")
                .Http()
                .Send(new HttpMessage<string, string>
                {
                    Endpoint = "test-endpoint",
                    Payload = "apples",
                    RequestType = HttpVerb.GET
                });

            Console.WriteLine("RESPONSE: " + response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        Thread.Sleep(3000);
    }
}
```
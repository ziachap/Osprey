using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Osprey.Tcp
{
    public class TcpConnection
    {
        public TcpConnection(TcpClient client, string remoteAddress)
        {
            TcpClient = client;
            Local = IPEndPoint.Parse(((IPEndPoint)TcpClient.Client.LocalEndPoint).ToString());
            Remote = IPEndPoint.Parse(remoteAddress);
            Lock = new object();
            
            Task.Run(async () =>
            {
                // TODO: look into whether chunk is going to be what you expect (length wise)
                await foreach(var chunk in TcpClient.GetAsyncStream())
                {
                    var lengthBuffer = chunk[..4];
                    var length = BitConverter.ToInt32(lengthBuffer);

                    var str = Encoding.ASCII.GetString(chunk[4..]);
                    //Console.WriteLine($"[{length}] {str}");

                    OnMessage?.Invoke(str);
                }
            });

            /*
            ListeningThread = new RepeatingTask(ct =>
            {
                using var stream = TcpClient.GetStream();
                while (!ct.IsCancellationRequested)
                {
                    while (!stream.DataAvailable)
                    {
                        if (ct.IsCancellationRequested) return;
                    }

                    var str = stream.ReceiveString(ct);
                    var message = str.DecodeMessage();

                    try
                    {
                        OnMessage?.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Failed to handle message");
                    }
                }
            });

            if (client.Connected) ListeningThread.Start();*/
        }
        
        public event Action<object> OnMessage;

        //protected RepeatingTask ListeningThread { get; set; }
        protected TcpClient TcpClient { get; set; }
        private object Lock { get; set; }

        public IPEndPoint Local { get; }
        public IPEndPoint Remote { get; }

        public virtual void Send(object message)
        {
            lock (Lock)
            {
                TcpClient.Client.Send(message);
            }
        }

        public void Dispose()
        {
            //ListeningThread.Stop();
            //ListeningThread?.Dispose();
            TcpClient?.Dispose();
        }
    }
}

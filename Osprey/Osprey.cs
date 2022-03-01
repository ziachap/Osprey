﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fclp;
using Newtonsoft.Json;
using Osprey.Configuration;
using Osprey.Logging;
using Osprey.Serialization;
using Osprey.Utilities;
using JsonSerializer = Osprey.Serialization.JsonSerializer;

namespace Osprey
{
	public class OSPREY : IDisposable
    {
        private static OSPREY _instance = null;

        /// <summary>
        /// Access the singleton instance of the connected Osprey network.
        /// </summary>
        public static OSPREY Network => _instance ?? throw new Exception("Caller has not joined an Osprey network.");

        public Node Node { get; private set; }
        public ISerializer Serializer { get; set; }
        public IOspreyLogger Logger { get; set; }
        public OspreyConfiguration Config { get; private set; }

        private OSPREY()
        {
            Config = new OspreyConfiguration();
            Serializer = new JsonSerializer();
            Logger = new ConsoleOspreyLogger();
        }

        /// <summary>
        /// Overwrites the configuration with an osprey configuration file.
        /// </summary>
        private void LoadJsonConfiguration(string filepath)
        {
            try
            {
                var file = File.ReadAllText(filepath);
                Config = JsonConvert.DeserializeObject<OspreyConfiguration>(file);
            }
            catch (FileNotFoundException ex)
            {
                Logger.Warn("No osprey configuration file found.");
            }
        }
        
        /// <summary>
        /// Overrides the configuration with any specified command line arguments 
        /// </summary>
        private void LoadCommandLineArguments(string[] args)
        {
            var p = new FluentCommandLineParser();

            //TODO: Complete these arguments

            p.Setup<string>('r', "udp-remote")
                .WithDescription("")
                .Callback(value => Config.Network.UdpBroadcastRemote = value);

            p.Setup<int>('p', "udp-remote-port")
                .WithDescription("")
                .Callback(value => Config.Network.UdpBroadcastPort = value);

            p.Setup<bool>('d', "use-dns-address")
                .WithDescription("")
                .Callback(value => Config.Network.UseDnsAddress = value);

            p.Parse(args);
        }

        /// <summary>
        /// Join the Osprey network.
        /// </summary>
        /// <param name="service">The name of the service.</param>
        /// <param name="environment">The environment to be isolated within.</param>
        /// <param name="configuration">An action to modify the Osprey instance before joining.</param>
        /// <returns>Disposable osprey instance.</returns>
        public static OSPREY Join(string service, string environment, Action<OSPREY> configuration = null)
		{
            if (_instance != null) throw new Exception("Cannot join the network more than once.");
            
            var osprey = new OSPREY();
            _instance = osprey;

            osprey.LoadJsonConfiguration("osprey.json");
            osprey.LoadCommandLineArguments(Environment.GetCommandLineArgs());
            
			var id = Guid.NewGuid().ToString();

            configuration?.Invoke(osprey);

            osprey.Node = new Node(id, service, environment);
            
            osprey.Node.Start();
            
            return osprey;
        }

        /// <summary>
        /// Attempt to locate a service on the network.
        /// </summary>
        public NodeInfo Locate(string node, bool throwError = false)
        {
            if (Node == null) throw new Exception("Caller has not joined an Osprey network.");

            return Node.Receiver.Locate(node, throwError);
        }

        /// <summary>
        /// Locate all instances of a service on the network.
        /// </summary>
        public IEnumerable<NodeInfo> LocateAll(string node)
        {
            return Node.Receiver.LocateAll(node);
        }

        /// <summary>
        /// Register a new service to be broadcasted on the network.
        /// </summary>
        public void Register(string type, string name, string address)
        {
            var service = new ServiceInfo()
            {
                Type = type,
                Name = name,
                Address = address
            };

            if (Node.Info.Services.Any(x => x.Name == name))
                throw new Exception("Cannot use the same service name multiple times.");

            Node.Info.Services.Add(service);
        }

        public void Dispose()
        {
            Node.Dispose();
            Node = null;
            Serializer = null;
            _instance = null;
        }
    }
}

using System.Net.WebSockets;
using Ajuna.NetApi;
using Ajuna.SDK.Demos.NetApi.Generated;
using Ajuna.SDK.Demos.NetApi.Generated.Model.FrameSystem;

namespace Ajuna.SDK.Demos.ServicePolling
{
    internal static class Program
    {
        private static string NodeUrl = "ws://127.0.0.1:9944";
        public static async Task Main(string[] args)
        {
            // Instantiate the client
            var client = new SubstrateClientExt(new Uri(NodeUrl));

            // Display Client Connection Status before connecting
            Console.WriteLine( $"Client Connection Status: {GetClientConnectionStatus(client)}");

            await client.ConnectAsync();
           
            // Display Client Connection Status after connecting
            Console.WriteLine( client.IsConnected ? "Client connected successfully" : "Failed to connect to node. Exiting...");
            
            if (!client.IsConnected)
                return;
            
            // Poll for Number Changes
            var continuePolling = true;
            
            Console.WriteLine($"Starting Number Value Polling");

            while (continuePolling)
            {
                string parameters = SystemStorage.NumberParams();
                var num = await client.GetStorageAsync<Ajuna.NetApi.Model.Types.Primitive.U32>(parameters, new CancellationToken());                
                Console.WriteLine($"Number is: {num.Value}");

                Thread.Sleep(2000);
                 
                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    continuePolling = false;
                }
            }
            
            Console.WriteLine($"Number Value Polling finished");
            
        }

        private static string GetClientConnectionStatus(SubstrateClient client)
        {
            return client.IsConnected ? "Connected" : "Not connected";

        }
    }
}
using Ajuna.NetApi;
using Serilog;
using SubstrateNET.NetApi.Generated;
using SubstrateNET.NetApi.Generated.Storage;

namespace Ajuna.SDK.Demos.DirectPolling
{
    internal static class Program
    {
        private static readonly ILogger Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
        
        private static string NodeUrl = "ws://127.0.0.1:9944";
        public static async Task Main(string[] args)
        {
            // Instantiate the client
            var client = new SubstrateClientExt(new Uri(NodeUrl));

            // Display Client Connection Status before connecting
            Logger.Information( $"Client Connection Status: {GetClientConnectionStatus(client)}");

            await client.ConnectAsync();
           
            // Display Client Connection Status after connecting
            Logger.Information( client.IsConnected ? "Client connected successfully" : "Failed to connect to node. Exiting...");
            
            if (!client.IsConnected)
                return;
            
            // Poll for Number Changes
            var continuePolling = true;
            
            Logger.Information($"Starting Block Number Polling");

            while (continuePolling)
            {
                string parameters = SystemStorage.NumberParams();
                var num = await client.GetStorageAsync<Ajuna.NetApi.Model.Types.Primitive.U32>(parameters, new CancellationToken());   
                
                Logger.Information($"Block Number is: {num.Value}");

                Thread.Sleep(2000);
                 
                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    continuePolling = false;
                }
            }
            
            Logger.Information($"Block Number Value Polling finished");
        }

        private static string GetClientConnectionStatus(SubstrateClient client)
        {
            return client.IsConnected ? "Connected" : "Not connected";
        }
    }
}
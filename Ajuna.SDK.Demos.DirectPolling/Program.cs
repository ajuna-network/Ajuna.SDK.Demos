using Ajuna.NetApi;
using Serilog;
using SubstrateNET.NetApi.Generated;

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
            var client = await InstantiateClientAndConnectAsync();
               
            if (!client.IsConnected)
                return;
            
            Logger.Information($"Starting Block Number Polling");

            while (true)
            {
                var scaleNum = await client.SystemStorage.Number(CancellationToken.None);
                Logger.Information("Block Number: {BlockNumber}", scaleNum.Value);
                Thread.Sleep(3000);
            }
        }

        private static async Task<SubstrateClientExt>  InstantiateClientAndConnectAsync()
        {
            // Instantiate the client
            var client = new SubstrateClientExt(new Uri(NodeUrl));

            // Display Client Connection Status before connecting
            Logger.Information( $"Client Connection Status: {GetClientConnectionStatus(client)}");

            await client.ConnectAsync();
           
            // Display Client Connection Status after connecting
            Logger.Information(client.IsConnected ? "Client connected successfully" : "Failed to connect to node. Exiting...");

            return client;
        }

        private static string GetClientConnectionStatus(SubstrateClient client)
        {
            return client.IsConnected ? "Connected" : "Not connected";
        }
    }
}
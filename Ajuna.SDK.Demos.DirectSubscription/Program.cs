using Ajuna.NetApi;
using Ajuna.NetApi.Model.Rpc;
using Serilog;
using SubstrateNET.NetApi.Generated;
using SubstrateNET.NetApi.Generated.Storage;


namespace Ajuna.SDK.Demos.DirectSubscription
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

           // Subscribe to the changes of System.Number by registering a Callback for each Number Change  
           await client.SubscribeStorageKeyAsync(SystemStorage.NumberParams(),
                   CallBackNumberChange, CancellationToken.None);

           Console.ReadLine();
        }

        /// <summary>
        /// Called on any number change.
        /// </summary>
        /// <param name="subscriptionId">The subscription identifier.</param>
        /// <param name="storageChangeSet">The storage change set.</param>
        private static void CallBackNumberChange(string subscriptionId, StorageChangeSet storageChangeSet)
        {
            if (storageChangeSet.Changes == null 
                || storageChangeSet.Changes.Length == 0 
                || storageChangeSet.Changes[0].Length < 2)
            {
                Logger.Error("Couldn't update account information. Please check 'CallBackAccountChange'");
                return;
            }

            
            var hexString = storageChangeSet.Changes[0][1];

            if (string.IsNullOrEmpty(hexString))
            {
                return;
            }
            
            var primitiveBlockNumber = new NetApi.Model.Types.Primitive.U32();
            primitiveBlockNumber.Create(Utils.HexToByteArray(hexString));

            Logger.Information($"New Block Number: {primitiveBlockNumber.Value}" );
        }

        private static string GetClientConnectionStatus(SubstrateClient client)
        {
            return client.IsConnected ? "Connected" : "Not connected";

        }
    }
}
using Ajuna.NetApi;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.SDK.Demos.NetApi.Generated;
using Serilog;


namespace Ajuna.SDK.Demos.DirectSubscription
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

           // Subscribe to the changes of System.Number by registering a Callback for each Number Change  
           await client.SubscribeStorageKeyAsync(Ajuna.SDK.Demos.NetApi.Generated.Model.FrameSystem.SystemStorage.NumberParams(),
                   CallBackNumberChange, new CancellationTokenSource().Token);

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
                // Logger.Warn("Couldn't update account informations. Please check 'CallBackAccountChange'");
                return;
            }

            var numberInfoString = storageChangeSet.Changes[0][1];

            if (string.IsNullOrEmpty(numberInfoString))
            {
                return;
            }

            Console.WriteLine("New Number Change: " + numberInfoString);
        }

        private static string GetClientConnectionStatus(SubstrateClient client)
        {
            return client.IsConnected ? "Connected" : "Not connected";

        }
    }
}
using System.Net.WebSockets;
using Ajuna.NetApi;
using Ajuna.ServiceLayer.Model;
using Serilog;
using SubstrateNET.RestClient;
using SubstrateNET.RestClient.Generated.Clients;

namespace Ajuna.SDK.Demos.ServiceSubscription
{
    internal static class Program
    {
        private static readonly ILogger Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
        
        // Websocket and API addresses of the Service layer - You need Ajuna.SDK.Demos.RestService running for this console app to run
        private static string WebsocketUrl = "ws://localhost:61752/ws";
        private static string ServiceUrl = "http://localhost:61752/";
        
        public static async Task Main(string[] args)
        {
            // Create BaseSubscriptionClient and connect
            var subscriptionClient = new BaseSubscriptionClient(new ClientWebSocket());
            await subscriptionClient.ConnectAsync(new Uri(WebsocketUrl), CancellationToken.None);
             
            // Assign Generic Handler for Storage Change
            subscriptionClient.OnStorageChange = HandleChange;

            // Create HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(ServiceUrl)
            };
            
            // Create SystemControllerClient
            var systemControllerClient = new SystemControllerClient(httpClient, subscriptionClient);

            // Subscribe to Number Changes
            var subscribedSuccessfully = await systemControllerClient.SubscribeNumber();
            
            // Continue only if Subscription has succeeded
            if (subscribedSuccessfully)
            {
                Logger.Information("Successfully Subscribed. Now listening for Storage Changes..."); ;
            }
            else
            {
                Logger.Information("Subscription failed. Exiting...");
                return;
            }
            
            while (true)
            {
                await subscriptionClient.ReceiveNextAsync(CancellationToken.None);
            }
         
            // Close Websocket Connection 
            await subscriptionClient.CloseAsync(WebSocketCloseStatus.Empty,"",CancellationToken.None);
        }

        private static void HandleChange(StorageChangeMessage message)
        {
            var primitiveBlockNumber = new NetApi.Model.Types.Primitive.U32();
            primitiveBlockNumber.Create(Utils.HexToByteArray(message.Data));

            Logger.Information("New Block Number: " + primitiveBlockNumber.Value);
        }
    }
}
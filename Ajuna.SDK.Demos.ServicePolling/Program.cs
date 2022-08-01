using System.Net.WebSockets;
using Ajuna.ServiceLayer.Model;
using SubstrateNET.RestClient;
using SubstrateNET.RestClient.Generated.Clients;

namespace Ajuna.SDK.Demos.ServiceSubscription
{
    internal static class Program
    {
        // Websocket and API addresses of the Service layer - You need Ajuna.SDK.Demos.RestService running for this console app to run
        private static String WebsocketUrl = "ws://localhost:61752/ws";
        private static String ServiceUrl = "http://localhost:61752/";
        
        public static async Task Main(string[] args)
        {
            // Create BaseSubscriptionClient and connect
            var subscriptionClient = new BaseSubscriptionClient(new ClientWebSocket());
            await subscriptionClient.ConnectAsync(new Uri(WebsocketUrl), CancellationToken.None);
             
            // Create HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(ServiceUrl)
            };
            
            // Create SystemControllerClient
            var systemControllerClient = new SystemControllerClient(httpClient, subscriptionClient);
            
            // Poll for Number Changes
            var continuePolling = true;
            
            Console.WriteLine($"Starting Number Value Polling");

            while (continuePolling)
            {
                 var newNumber = await systemControllerClient.GetNumber();
                 Console.WriteLine($"Number is: {newNumber.Value}");
                 Thread.Sleep(2000);
                 
                 if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                 {
                     continuePolling = false;
                 }
            }
            
            Console.WriteLine($"Number Value Polling finished");
        }

        private static void HandleChange(StorageChangeMessage message)
        {
            Console.WriteLine("New Change: " + message.Data);
        }
    }
}
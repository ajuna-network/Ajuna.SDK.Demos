using System.Net.WebSockets;
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
        private static readonly string WebsocketUrl = "ws://localhost:61752/ws";
        private static readonly string ServiceUrl = "http://localhost:61752/";

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
            
            Logger.Information($"Starting Number Value Polling");

            while (true)
            {
                 var newNumber = await systemControllerClient.GetNumber();
                 Logger.Information($"Number is: {newNumber.Value}");
                 Thread.Sleep(3000);
            }
        }
    }
}
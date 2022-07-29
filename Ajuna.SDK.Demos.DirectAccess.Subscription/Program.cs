// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

namespace Ajuna.SDK.Demos.DirectAccess.Subscription
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            // Create BaseSubscriptionClient and connect
            var subscriptionClient = new BaseSubscriptionClient(new ClientWebSocket());
            await subscriptionClient.ConnectAsync(new Uri($"{GetBaseAddress().Replace("http", "ws")}/ws"),
                CancellationToken.None);

            // Assign Generic Handler for Storage Change
            subscriptionClient.OnStorageChange = HandleChange;

            // Create HttpClient
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(GetBaseAddress())
            };

            // Create SystemControllerClient
            var systemControllerClient = new SystemControllerClient(httpClient, subscriptionClient);

            // Subscribe to Number Changes
            var subscribedSuccessfully = await systemControllerClient.SubscribeNumber();

            // Continue only if Subscription has succeeded
            if (subscribedSuccessfully)
            {
                Console.WriteLine("Successfully Subscribed. Now listening for Storage Changes...");
                Console.WriteLine("Press ESC to exit");
            }
            else
            {
                Console.WriteLine("Subscription failed. Exiting...");
                return;
            }

            // Keep reading the stream waiting for Storage Changes and exit when the user presses the ESCAPE key
            bool listenForStorageChanges = true;
            while (listenForStorageChanges)
            {
                await subscriptionClient.ReceiveNextAsync(CancellationToken.None);

                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    listenForStorageChanges = false;
                }
            }

            // Close Websocket Connection 
            await subscriptionClient.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }

        private static string GetBaseAddress()
        {
            return Environment.GetEnvironmentVariable("AJUNA_SERVICE_ENDPOINT") ?? "http://localhost:61752";
        }

        private static void HandleChange(StorageChangeMessage message)
        {
            Console.WriteLine("New Change: " + message.Data);
        }
    }
}
using Ajuna.NetApi;
using Ajuna.NetApi.Model.Types;
using Schnorrkel.Keys;
using Serilog;

namespace Ajuna.SDK.Demos.NetWallet
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
            var client = new Ajuna.NetApi.SubstrateClient(new Uri(NodeUrl));

            Logger.Information("Connected: {IsConnected}", client.IsConnected);

            await client.ConnectAsync();

            Logger.Information("Connected: {IsConnected}", client.IsConnected);

            Logger.Information("{SpecName}", client.RuntimeVersion.SpecName);
            Logger.Information("{ImplName}", client.RuntimeVersion.ImplName);
        }
    }
}
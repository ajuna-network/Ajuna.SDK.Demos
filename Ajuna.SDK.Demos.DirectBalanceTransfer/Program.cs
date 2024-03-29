﻿using Ajuna.NetApi;
using Ajuna.NetApi.Model.Extrinsics;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Schnorrkel.Keys;
using Serilog;
using SubstrateNET.NetApi.Generated;
using SubstrateNET.NetApi.Generated.Model.sp_core.crypto;
using SubstrateNET.NetApi.Generated.Model.sp_runtime.multiaddress;
using SubstrateNET.NetApi.Generated.Storage;

namespace Ajuna.SDK.Demos.DirectBalanceTransfer
{
    internal static class Program
    {
        // Secret Key URI `//Alice` is account:
        // Secret seed:      0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a
        // Public key(hex):  0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d
        // Account ID:       0xd43593c715fdd31c61141abd04a99fd6822c8558854ccde39a5684e7a56da27d
        // SS58 Address:     5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY
        public static MiniSecret MiniSecretAlice => new MiniSecret(
            Utils.HexToByteArray("0xe5be9a5092b81bca64be81d212e7f2f9eba183bb7a90954f7b76361f6edb5c0a"),
            ExpandMode.Ed25519);

        public static Account Alice => Account.Build(KeyType.Sr25519, MiniSecretAlice.ExpandToSecret().ToBytes(),
            MiniSecretAlice.GetPair().Public.Key);

        // Secret Key URI `//Bob` is account:
        // Secret seed:      0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89
        // Public key(hex):  0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48
        // Account ID:       0x8eaf04151687736326c9fea17e25fc5287613693c912909cb226aa4794f26a48
        // SS58 Address:     5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty
        public static MiniSecret MiniSecretBob => new MiniSecret(
            Utils.HexToByteArray("0x398f0c28f98885e046333d4a41c19cee4c37368a9832c6502f6cfd182e2aef89"),
            ExpandMode.Ed25519);

        public static Account Bob => Account.Build(KeyType.Sr25519, MiniSecretBob.ExpandToSecret().ToBytes(),
            MiniSecretBob.GetPair().Public.Key);
        
        private static readonly ILogger Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
        
        private static string NodeUrl = "ws://127.0.0.1:9944";

        public static async Task Main(string[] args)
        {
            // Instantiate the client and connect to the Node
            SubstrateClientExt client = await InstantiateClientAndConnectAsync();

            if (!client.IsConnected)
                return;

            await SubmitTransfer(client);
        }

        public static async Task SubmitTransfer(SubstrateClientExt client)
        {
            var accountAlice = new AccountId32();
            accountAlice.Create(Utils.GetPublicKeyFrom(Alice.Value));

            var accountBob = new AccountId32();
            accountBob.Create(Utils.GetPublicKeyFrom(Bob.Value));
           
            // Get Alice's Balance
            var accountInfoAlice = await client.SystemStorage.Account(accountAlice, CancellationToken.None);
            Logger.Information("Alice Free Balance before transaction = {balance}",accountInfoAlice.Data.Free.Value.ToString());
           
            // Get Bob's Balance
            var accountInfoBob = await client.SystemStorage.Account(accountBob, CancellationToken.None);
            Logger.Information("Bob Free Balance before transaction = {balance}",accountInfoBob.Data.Free.Value.ToString());

            // Instantiate a MultiAddress for Bob
            var multiAddressBob = new EnumMultiAddress();
            multiAddressBob.Create(MultiAddress.Id, accountBob);

            // Amount to be transferred
            var amount = new BaseCom<U128>();
            amount.Create(190000);

            // Create Extrinsic Method to be transmitted
            var extrinsicMethod =
                BalancesCalls.Transfer(multiAddressBob, amount);

            // Post Extrinsic Callback to show balance for both accounts
            Action<string, ExtrinsicStatus> actionExtrinsicUpdate =  (subscriptionId, extrinsicUpdate) => 
                {
                    // Fire only if state is Ready
                    if (extrinsicUpdate.ExtrinsicState == ExtrinsicState.Ready)
                    {
                        Logger.Information("Firing post transfer Callback");

                        client.SystemStorage.Account(accountAlice, CancellationToken.None).ContinueWith(
                            (task) =>
                                Logger.Information("Alice's Free Balance after transaction = {balance}",task.Result.Data.Free.Value)
                            
                            );
                        
                        
                        client.SystemStorage.Account(accountBob, CancellationToken.None).ContinueWith(
                            (task) =>
                                Logger.Information("Bob's Free Balance after transaction = {balance}",task.Result.Data.Free.Value)
                            
                        );
                    }
                };

            // Alice to Bob Transaction
            await client.Author.SubmitAndWatchExtrinsicAsync(
                actionExtrinsicUpdate,
                extrinsicMethod,
                Alice, new ChargeAssetTxPayment(0, 0), 128, CancellationToken.None);


            Console.ReadLine();
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
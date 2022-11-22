using Ajuna.NetApi;
using Ajuna.NetApi.Model.Extrinsics;
using Ajuna.NetApi.Model.Rpc;
using Ajuna.NetApi.Model.Types;
using Ajuna.NetApi.Model.Types.Base;
using Ajuna.NetApi.Model.Types.Primitive;
using Ajuna.NetWallet;
using Schnorrkel.Keys;
using Serilog;
using SubstrateNET.NetApi.Generated;
using SubstrateNET.NetApi.Generated.Model.sp_core.crypto;
using SubstrateNET.NetApi.Generated.Model.sp_runtime.multiaddress;
using SubstrateNET.NetApi.Generated.Storage;

namespace Ajuna.SDK.Demos.NetWallet
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

        private static readonly ILogger Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();
        
        private static string NodeUrl = "ws://127.0.0.1:9944";
        
        public static async Task Main(string[] args)
        {
            // Setup for locally storing the wallet file
            SystemInteraction.ReadData = f => File.ReadAllText(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.DataExists = f => File.Exists(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.ReadPersistent = f => File.ReadAllText(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.PersistentExists = f => File.Exists(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.Persist = (f, c) => File.WriteAllText(Path.Combine(Environment.CurrentDirectory, f), c);
            
                
            string WalletName = "AjunaWallet";
            string WalletPassword = "aA1234dd";
            string MnemonicSeed = "caution juice atom organ advance problem want pledge someone senior holiday very";

            // Create Wallet 
            var wallet = new Wallet();
            wallet.Create(WalletPassword, MnemonicSeed, KeyType.Sr25519,
                Mnemonic.BIP39Wordlist.English, WalletName);

            if (!wallet.IsCreated)
            {
                Logger.Error("Could not create wallet. Exiting...");
                return;
            }
            
            Logger.Information("Wallet created successfully");

            // Load wallet from storage 
            var ajunaWallet = new Wallet();
            ajunaWallet.Load(WalletName);
            
            Logger.Information($"Wallet is unlocked: {ajunaWallet.IsUnlocked}");
            
            // Unlock 
            ajunaWallet.Unlock(WalletPassword);

            Logger.Information($"Wallet is unlocked: {ajunaWallet.IsUnlocked}");

            if (!wallet.IsUnlocked)
            {
                Logger.Error("Could not unlock wallet. Exiting...");
                return;
            }
            
            // Instantiate the client and connect to Node
            SubstrateClientExt client = await InstantiateClientAndConnectAsync();

            if (!client.IsConnected)
                return;

            // Alice is always generous and will send us a small gift
            var accountAlice = new AccountId32();
            accountAlice.Create(Utils.GetPublicKeyFrom(Alice.Value));
            
            var accountWallet = new AccountId32();
            accountWallet.Create(Utils.GetPublicKeyFrom(ajunaWallet.Account.Value));
            
            // Get Alice's Balance
            // Wallet' Account's has not been created yet and does not have a Balance
            var accountInfoAlice = await client.SystemStorage.Account(accountAlice, CancellationToken.None);
            Logger.Information($"Alice Free Balance before transaction = {accountInfoAlice.Data.Free.Value.ToString()}");
           
            // Transfer Money from Alice to Wallet
            await TransferAsync(client, Alice, wallet.Account, 400000000000000);
           
            // Wait for the transfer to complete
            Thread.Sleep(8000);

            // Let's check the accounts again after the transfer
            accountInfoAlice = await client.SystemStorage.Account(accountAlice, CancellationToken.None);
            Logger.Information($"Alice Free Balance after sending to Wallet = {accountInfoAlice.Data.Free.Value.ToString()}");
           
            var  accountInfoWallet = await client.SystemStorage.Account(accountWallet, CancellationToken.None);
            Logger.Information($"Wallet's Account Free Balance after receiving from Alice = {accountInfoWallet.Data.Free.Value.ToString()}");

            //Let's give something back to Alice!
            // Transfer Money from Alice to Wallet
            await TransferAsync(client,  wallet.Account, Alice,10000000000);
            
            Thread.Sleep(8000);

            // Let's check the accounts again after the transfer
            accountInfoAlice = await client.SystemStorage.Account(accountAlice, CancellationToken.None);
            Logger.Information($"Alice Free Balance after receiving from Wallet = {accountInfoAlice.Data.Free.Value.ToString()}");
           
            accountInfoWallet = await client.SystemStorage.Account(accountWallet, CancellationToken.None);
            Logger.Information($"Wallets Account Free Balance after sending to Alice = {accountInfoWallet.Data.Free.Value.ToString()}");

            Console.ReadLine();
        }
        
        private static async Task TransferAsync(SubstrateClientExt client, Account senderAccount,Account recipientAccount,  long amount)
        {
            if (!client.IsConnected)
            {
                Logger.Debug("Not connected!");
            }
        
            var transferRecipient = new AccountId32();
            transferRecipient.Create(recipientAccount.Bytes);
        
            var multiAddress = new EnumMultiAddress();
            multiAddress.Create(MultiAddress.Id, transferRecipient);
        
            var baseCampactU128 = new BaseCom<U128>();
            baseCampactU128.Create(amount);
        
            var balanceTransferMethod =
                BalancesCalls.Transfer(multiAddress, baseCampactU128);
            
            Action<string, ExtrinsicStatus> actionExtrinsicUpdate = (subscriptionId, extrinsicUpdate) => 
                Logger.Information($"Extrinsic CallBack[{subscriptionId}]: {extrinsicUpdate}");

            await client.Author.SubmitAndWatchExtrinsicAsync(
                actionExtrinsicUpdate,
                balanceTransferMethod,
                senderAccount, new ChargeAssetTxPayment(0, 0), 64, CancellationToken.None);
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
using Ajuna.NetApi;
using Ajuna.NetApi.Model.Extrinsics;
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

        private static string _walletName = "AjunaWallet";
        private static string _walletPassword = "aA1234dd";

        public static async Task Main(string[] args)
        {
            // Instantiate the client
            var client = new SubstrateClientExt(new Uri(NodeUrl));

            // Display Client Connection Status before connecting
            Logger.Information($"Client Connection Status: {GetClientConnectionStatus(client)}");

            await client.ConnectAsync();

            // Display Client Connection Status after connecting
            Logger.Information(client.IsConnected
                ? "Client connected successfully"
                : "Failed to connect to node. Exiting...");

            if (!client.IsConnected)
                return;

            await DoEverything(client);
        }
        
        internal static string CreateMnemonicSeed()
        {
            var randomBytes = new byte[16];
            new Random().NextBytes(randomBytes);
            var mnemonicSeed = string.Join(' ', Mnemonic.MnemonicFromEntropy(randomBytes, Mnemonic.BIP39Wordlist.English));
            return mnemonicSeed;
        }

        public static async Task DoEverything(SubstrateClientExt client)
        {
            // Setup Storage
            SystemInteraction.ReadData = f => File.ReadAllText(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.DataExists = f => File.Exists(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.ReadPersistent = f => File.ReadAllText(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.PersistentExists = f => File.Exists(Path.Combine(Environment.CurrentDirectory, f));
            SystemInteraction.Persist = (f, c) => File.WriteAllText(Path.Combine(Environment.CurrentDirectory, f), c);

            
            // Create Wallet 
            var wallet = new Wallet();
            wallet.Create(_walletPassword, CreateMnemonicSeed(), KeyType.Sr25519,
                Mnemonic.BIP39Wordlist.English, _walletName);
                
                
                //Create(_walletPassword, walletName:_walletName);

            if (!wallet.IsCreated)
            {
                Logger.Error("Could not create wallet. Exiting...");
                return;
            }
            
            Logger.Information("Wallet created successfully");

            // Load the wallet we created 
            var ajunaWallet = new Wallet();
            ajunaWallet.Load(_walletName);
            
            Logger.Information($"Wallet is unlocked: {ajunaWallet.IsUnlocked}");
            
            // Unlock 
            ajunaWallet.Unlock(_walletPassword);
            
            Logger.Information($"Wallet is unlocked: {ajunaWallet.IsUnlocked}");

            if (!wallet.IsUnlocked)
            {
                Logger.Error("Could not unlock wallet. Exiting...");
                return;
            }

            // Alice is always generous and will send us a small gift
            var accountAlice = new AccountId32();
            accountAlice.Create(Utils.GetPublicKeyFrom(Alice.Value));
            
            var accountWallet = new AccountId32();
            accountWallet.Create(Utils.GetPublicKeyFrom(ajunaWallet.Account.Value));
            
            // Get Alice's Balance
            var accountInfoAlice = await client.SystemStorage.Account(accountAlice, CancellationToken.None);
            Logger.Information($"Alice Free Balance before transaction = {accountInfoAlice.Data.Free.Value.ToString()}");
           
             // Get Wallet Account's Balance
             // var accountInfoWalletBeforeBeingCreated = await client.SystemStorage.Account(accountWallet, CancellationToken.None);
             // Logger.Information($"Wallets Account Free Balance before transaction = {accountInfoWalletBeforeBeingCreated.Data.Free.Value.ToString()}");
            
            // Transfer Money from Alice to Wallet
            await TransferAsync(client, Alice, wallet.Account, 400000000000000);
            
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

        // public async Task GetBalanceAsync(SubstrateClientExt client, Account account)
        // {
        //     accountInfoAlice = await client.SystemStorage.Account(, CancellationToken.None);
        //     Logger.Information($"Alice Free Balance after transaction = {accountInfoAlice.Data.Free.Value}");
        // }
        
        public static async Task TransferAsync(SubstrateClientExt client, Account senderAccount,Account recipientAccount,  long amount)
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
        
            var transferKeepAlive = BalancesCalls.TransferKeepAlive(multiAddress, baseCampactU128);
            
            var extrinsicMethod =
                BalancesCalls.Transfer(multiAddress, baseCampactU128);
        
            var subscriptionId = await client.Author.SubmitExtrinsicAsync(
                // transferKeepAlive,
                extrinsicMethod,
                senderAccount, new ChargeAssetTxPayment(0, 0), 64, CancellationToken.None);
        }

        private static string GetClientConnectionStatus(SubstrateClient client)
        {
            return client.IsConnected ? "Connected" : "Not connected";
        }
    }
}
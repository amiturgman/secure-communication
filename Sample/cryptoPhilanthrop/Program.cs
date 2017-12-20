using System;
using System.Configuration;
using System.Threading;
using SecuredCommunication;
using Microsoft.Azure.KeyVault.Models;
using Contracts;

namespace CoinsSender
{
    /// <summary>
    ///  A sample app that checks balance and while > some value
    ///  keep asking the transferee to create more transactions (Sends money)
    /// </summary>
    class Program
    {
        #region private members

        private const string c_ReciverId = "reciverAccount";
        private const string c_senderId = "senderAccount";

        #endregion

        static void Main(string[] args)
        {
            var kv = new KeyVault(ConfigurationManager.AppSettings["AzureKeyVaultUri"],
                ConfigurationManager.AppSettings["applicationId"], ConfigurationManager.AppSettings["applicationSecret"]);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kv, ConfigurationManager.AppSettings["EthereumNodeUrl"]);
            
            while (true)
            {
                Console.WriteLine("To run the demo with Ethereum Testnet press 1");
                Console.WriteLine("To run the demo with Docker testrpc press 2");
                
                Console.WriteLine("Press any other key to exit");
                Console.WriteLine();

                var userInput = double.Parse(Console.ReadLine());

                switch (userInput)
                {
                    case 1:
                        EthereumTestnetDemo(kv, ethereumNodeWrapper);
                        continue;
                    case 2:
                        EthereumTestRpcDemo(kv, ethereumNodeWrapper);
                        continue;
                    default:
                        return;
                }
            }
        }

        private static void EthereumTestRpcDemo(KeyVault kv, EthereumNodeWrapper ethereumNodeWrapper)
        {
            var senderPublicAddress = "0xe6128e8d408f53ea53e74be796d40db896fcaef0";
            var senderPrivateKey = "0x4faec59e004fd62384813d760e55d6df65537b4ccf62f268253ad7d4243a7193";
            var reciverPublicAddress = "0x9108cf23b4f60f2bc355088a51539b576c7f9e6d";
            var reciverPrivateKey = "0x03fd5782c37523be6598ca0e5d091756635d144e42d518bb5f8db11cf931b447";

            Console.WriteLine($"Please run the docker image with the following command:{Environment.NewLine}"+
                "docker run -d -p 8545:8545 trufflesuite/ganache-cli:latest " +
                $"--account=\"{senderPrivateKey}, 300000000000000000000\"" +
                $" --account=\"{reciverPrivateKey}, 0\"");
            Console.WriteLine("Press enter once the docker is running");
            Console.ReadLine();

            // Check if Account already stored in KeyVault
            try
            {
                var senderAccount = ethereumNodeWrapper.GetPublicKeyAsync(c_senderId).Result;
                var reciverAccount = ethereumNodeWrapper.GetPublicKeyAsync(c_ReciverId).Result;

            } catch (Exception ex)
            {
                if (ex.InnerException is KeyVaultErrorException && ex.InnerException.Message.Contains("Secret not found"))
                {
                    // Create accounts
                    var senderAccount= new EthKey(senderPrivateKey, Utils.ToByteArray(senderPublicAddress), senderPublicAddress);
                    var reciverAccount =  new EthKey(reciverPrivateKey, Utils.ToByteArray(senderPublicAddress), reciverPublicAddress);

                    var result = ethereumNodeWrapper.StoreAccountAsync(c_senderId, senderAccount).Result;
                    result = ethereumNodeWrapper.StoreAccountAsync(c_ReciverId, reciverAccount).Result;
                }
            } finally
            {
                SendCoins(kv, ethereumNodeWrapper);
            }
        }

        private static void EthereumTestnetDemo(KeyVault kv, EthereumNodeWrapper ethereumNodeWrapper)
        {
            while (true)
            {
                Console.WriteLine("To create new accounts press 1");
                Console.WriteLine("If you already created sender and receiver accounts press 2");
                var input = double.Parse(Console.ReadLine());
                switch (input)
                {
                    case 1:
                        // Create accounts
                        var senderAccount = ethereumNodeWrapper.CreateAccount();
                        var result = ethereumNodeWrapper.StoreAccountAsync(c_senderId, senderAccount).Result;
                        var reciverAccount = ethereumNodeWrapper.CreateAccount();
                        result = ethereumNodeWrapper.StoreAccountAsync(c_ReciverId, reciverAccount).Result;

                        Console.WriteLine("Accounts were created. " +
                                          $"To continue the demo please send ether to address {senderAccount.PublicAddress}{Environment.NewLine}" +
                                          "You can send ether for: https://www.rinkeby.io/#faucet");
                        continue;
                    case 2:
                        SendCoins(kv, ethereumNodeWrapper);
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SendCoins(KeyVault kv, EthereumNodeWrapper ethereumNodeWrapper)
        {
            Console.WriteLine("Sender - Happy to transfer my crypto coins!");

            // Init
            var senderAddress = ethereumNodeWrapper.GetPublicKeyAsync(c_senderId).Result;
            var reciverAddress = ethereumNodeWrapper.GetPublicKeyAsync(c_ReciverId).Result;
            var balance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
            PrintCurrentBalance(senderAddress, balance);

            var encryptionKeyName = ConfigurationManager.AppSettings["EncryptionKeyName"];
            var decryptionKeyName = ConfigurationManager.AppSettings["DecryptionKeyName"];
            var signKeyName = ConfigurationManager.AppSettings["SignKeyName"];
            var verifyKeyName = ConfigurationManager.AppSettings["VerifyKeyName"];

            var secretsMgmnt = new KeyVaultSecretManager(encryptionKeyName, decryptionKeyName, signKeyName, verifyKeyName, kv, kv);
            secretsMgmnt.Initialize().Wait();
            //var securedComm = new RabbitMQBusImpl(ConfigurationManager.AppSettings["rabbitMqUri"], secretsMgmnt, true, "securedCommExchange");
            var securedComm = new AzureQueueImpl("transactions", ConfigurationManager.AppSettings["AzureStorageConnectionString"], secretsMgmnt, true);
            securedComm.Initialize().Wait();

            // While there are sufficient funds, transfer some...
            while (balance > 0)
            {
                var amountToSend = 0.001;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                securedComm.EnqueueAsync(
                    $"{amountToSend};{c_senderId};{reciverAddress}").Wait();

                // Sleep 1 minute
                Thread.Sleep(60000);

                var newBalance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
                PrintCurrentBalance(senderAddress, newBalance);

                // Wait for mining.. 
                while (newBalance.Equals(balance))
                {
                    newBalance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
                }

                balance = newBalance;
            }
        }

        public static void PrintCurrentBalance(string address, decimal balance)
        {
            Console.WriteLine($"Account {address} balance: {balance}");
        }
    }
}

using System;
using System.Configuration;
using System.Threading;
using Microsoft.Azure.KeyVault.Models;
using Wallet.Blockchain;
using Wallet.Communication;
using Wallet.Communication.AzureQueueDependencies;
using Wallet.Cryptography;

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
            var ethereumNodeWrapper = new EthereumAccount(kv, ConfigurationManager.AppSettings["EthereumNodeUrl"]);
            
            while (true)
            {
                Console.WriteLine("To run the demo with Ethereum Testnet press 1");
                Console.WriteLine("To run the demo with Docker TestRpc press 2");
                
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

        private static void EthereumTestRpcDemo(KeyVault kv, EthereumAccount ethereumAccount)
        {
            var senderPrivateKey = "0x4faec59e004fd62384813d760e55d6df65537b4ccf62f268253ad7d4243a7193";
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
                var senderAccount = ethereumAccount.GetPublicAddressAsync(c_senderId).Result;
                var reciverAccount = ethereumAccount.GetPublicAddressAsync(c_ReciverId).Result;

            } catch (Exception ex)
            {
                if (ex.InnerException is KeyVaultErrorException && ex.InnerException.Message.Contains("Secret not found"))
                {
                    ethereumAccount.CreateAccountAsync(c_senderId, senderPrivateKey).Wait();
                    ethereumAccount.CreateAccountAsync(c_ReciverId, reciverPrivateKey).Wait();
                }
            } finally
            {
                SendCoins(kv, ethereumAccount);
            }
        }

        private static void EthereumTestnetDemo(KeyVault kv, EthereumAccount ethereumAccount)
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
                        ethereumAccount.CreateAccountAsync(c_senderId).Wait();
                        ethereumAccount.CreateAccountAsync(c_ReciverId).Wait();

                        var senderPublicAddress = ethereumAccount.GetPublicAddressAsync(c_senderId).Result;
                        Console.WriteLine("Accounts were created. " +
                                          $"To continue the demo please send ether to address {senderPublicAddress}{Environment.NewLine}" +
                                          "You can send ether for: https://www.rinkeby.io/#faucet");
                        continue;
                    case 2:
                        SendCoins(kv, ethereumAccount);
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SendCoins(KeyVault kv, EthereumAccount ethereumAccount)
        {
            Console.WriteLine("Sender - Happy to transfer my crypto coins!");

            // Init
            var senderAddress = ethereumAccount.GetPublicAddressAsync(c_senderId).Result;
            var reciverAddress = ethereumAccount.GetPublicAddressAsync(c_ReciverId).Result;
            var balance = ethereumAccount.GetCurrentBalance(senderAddress).Result;
            PrintCurrentBalance(senderAddress, balance);

            var encryptionKeyName = ConfigurationManager.AppSettings["EncryptionKeyName"];
            var decryptionKeyName = ConfigurationManager.AppSettings["DecryptionKeyName"];
            var signKeyName = ConfigurationManager.AppSettings["SignKeyName"];
            var verifyKeyName = ConfigurationManager.AppSettings["VerifyKeyName"];

            var secretsMgmnt = new KeyVaultCryptoActions(encryptionKeyName, decryptionKeyName, signKeyName, verifyKeyName, kv, kv);
            secretsMgmnt.Initialize().Wait();
            //var securedComm = new RabbitMQBusImpl(ConfigurationManager.AppSettings["rabbitMqUri"], secretsMgmnt, true, "securedCommExchange");

            var queueClient = new CloudQueueClientWrapper(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            var securedComm = new AzureQueue("transactions", queueClient, secretsMgmnt, true);
            securedComm.Initialize().Wait();

            // While there are sufficient funds, transfer some...
            while (balance > 0)
            {
                var amountToSend = 0.001;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                var message = $"{amountToSend};{c_senderId};{reciverAddress}";
                securedComm.EnqueueAsync(Utils.ToByteArray(message)).Wait();

                // Sleep 1 minute
                Thread.Sleep(60000);

                var newBalance = ethereumAccount.GetCurrentBalance(senderAddress).Result;
                PrintCurrentBalance(senderAddress, newBalance);

                // Wait for mining.. 
                while (newBalance.Equals(balance))
                {
                    newBalance = ethereumAccount.GetCurrentBalance(senderAddress).Result;
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

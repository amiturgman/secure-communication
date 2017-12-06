using System;
using System.Threading;
using SecuredCommunication;
using System.Configuration;
using Contracts;

namespace cryptoPhilanthrop
{
    /// <summary>
    ///  A sample app that checks balance and while > some value
    ///  keep asking the transferer to do more transactions
    /// </summary>
    class Program
    {
        #region private members

        private const string c_keyVaultUri = "https://eladiw-testkv.vault.azure.net/";
        private const string c_ethereumTestNodeUrl = "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE";
        private const string c_encKeyName = "enc_public";
        private const string c_decKeyName = "dec_private";
        private const string c_signKeyName = "sign_private";
        private const string c_verifyKeyName = "verify_public";
        private const string c_senderId = "account1testnent";
        private const string c_ReciverId = "account2testnent";

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Sender - Happy to transfer my crypto coins!");

            // Init
            var kvInfo = new KeyVault(c_keyVaultUri);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kvInfo, c_ethereumTestNodeUrl);

            var senderAddress = kvInfo.GetPublicKeyAsync(c_senderId).Result;
            var reciverAddress = kvInfo.GetPublicKeyAsync(c_ReciverId).Result;
            var balance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
            PrintCurrentBalance(senderAddress, balance);

            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new RabbitMQBusImpl(secretsMgmnt, uri, c_verifyKeyName, c_signKeyName, false, c_encKeyName, c_decKeyName);

            // While there are sufficient funds, transfer some...
            while (balance > 0)
            {
                var amountToSend = 0.001;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                securedComm.EnqueueAsync(
                    "transactions",
                    new Message($"{amountToSend};{c_senderId};{reciverAddress}")).Wait();

                // Sleep 10 minutes
                Thread.Sleep(600000);

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

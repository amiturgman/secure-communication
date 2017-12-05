using System;
using System.Threading;
using SecuredCommunication;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Configuration;

namespace cryptoPhilanthrop
{
    /// <summary>
    ///  A sample app that checks balance and while > some value
    ///  keep asking the transferer to do more transactions
    /// </summary>
    class Program
    {
        #region private members

        private const string c_localKey = @"C:\temp\NetherumDemo\privchain\keystore\UTC--2017-12-05T14-15-41.212709700Z--7a9758b84b851b3acfcd36ea1fccb054cbfcf257";
        private const string c_keyVaultUri = "https://eladiw-testkv.vault.azure.net/";
        private const string c_encKeyName = "enc_public";
        private const string c_decKeyName = "dec_private";
        private const string c_signKeyName = "sign_private";
        private const string c_verifyKeyName = "verify_public";
        private const string c_password = "12345678";

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Sender - Happy to transfer my crypto coins!");
         
            // Init
            var account = Account.LoadFromKeyStore(File.ReadAllText(c_localKey), c_password);

            var balance = EthereumNodeWrapper.GetCurrentBalance(account.Address).Result;
            PrintCurrentBalance(account, balance);
            var newBalance = balance;

            var kvInfo = new KeyVault(c_keyVaultUri);
            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new RabbitMQBusImpl(secretsMgmnt, uri, c_verifyKeyName, c_signKeyName, false, c_encKeyName, c_decKeyName);

            // While there are sufficient funds, transfer some...
            while (balance > 1)
            {
                var amountToSend = 5000;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                securedComm.EnqueueAsync(
                    "transactions",
                    new Message($"{amountToSend};sender;0xba0c386f5e72d9bd06ff2da9feec57497e8ce582")).Wait();

                Thread.Sleep(60000);
                
                newBalance = EthereumNodeWrapper.GetCurrentBalance(account.Address).Result;
                PrintCurrentBalance(account, newBalance);

                // Wait for mining.. 
                while (newBalance.Equals(balance))
                {
                    newBalance = EthereumNodeWrapper.GetCurrentBalance(account.Address).Result;
                }

                balance = newBalance;
            }
        }

        public static void PrintCurrentBalance(Account account, decimal balance)
        {
            Console.WriteLine($"Account {account.Address} balance: {balance}");
        }
    }
}

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

        private const string c_localKey = @"C:\temp\NetherumDemo\privchain\keystore\UTC--2017-11-30T13-34-42.742317500Z--bb6d204b166279511ce6cb4547275e805bc8cb82";
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
            var securedComm = new SecuredComm(secretsMgmnt, uri, c_verifyKeyName, c_signKeyName, false, c_encKeyName, c_decKeyName);

            // While there are sufficient funds, transfer some...
            while (balance > 10000)
            {
                var amountToSend = 5000;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                securedComm.SendMsgAsync(
                    "sender.transactions",
                    new Message($"{amountToSend};sender;0x863c813c74acee5e4063bd65e880c0f06d3cc765")).Wait();

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

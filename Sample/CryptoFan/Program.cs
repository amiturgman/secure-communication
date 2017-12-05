using System;
using System.Threading;
using SecuredCommunication;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Configuration;

namespace CryptoFan
{
    /// <summary>
    /// A sample app that registers on a queue, whenver it gets update
    /// it checks and prints the balance.
    /// </summary>
    class Program
    {
        #region private members

        private const string c_localKey = @"C:\temp\NetherumDemo\privchain\keystore\UTC--2017-11-30T13-36-01.594748200Z--863c813c74acee5e4063bd65e880c0f06d3cc765";
        private const string c_password = "12345678";
        private const string c_keyVaultUri = "https://eladiw-testkv.vault.azure.net/";
        private const string c_encKeyName = "enc_public";
        private const string c_decKeyName = "dec_private";
        private const string c_signKeyName = "sign_private";
        private const string c_verifyKeyName = "verify_public";
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Reciever - I just love getting new crypto coins");

            // Init
            var account = Account.LoadFromKeyStore(File.ReadAllText(c_localKey), c_password);

            PrintCurrentBalance(account, EthereumNodeWrapper.GetCurrentBalance(account.Address).Result);

            var kvInfo = new KeyVault(c_keyVaultUri);
            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new SecuredComm(secretsMgmnt, uri, c_verifyKeyName, c_signKeyName, false, c_encKeyName, c_decKeyName);

            // Listen on the notifications queue, check balance when a notification arrives
            var consumerTag =
                securedComm.ListenOnQueue("notifications",
                                          new string[] { "notifications.balance" },
                                          (msg) =>
                                          {
                                              if (msg.data.Equals(account.Address, StringComparison.OrdinalIgnoreCase))
                                              {
                                                  Console.WriteLine("Great, Balance change!");
                                                  PrintCurrentBalance(account, EthereumNodeWrapper.GetCurrentBalance(account.Address).Result);
                                              }
                                              else
                                              {
                                                  Console.WriteLine("Not my balance!");
                                                  Console.WriteLine(msg.data);
                                              }
                                          });

            // wait 30 minutes
            Thread.Sleep(30 * 1000 * 60);

            securedComm.CancelListeningOnQueue(consumerTag);
        }

        private static void PrintCurrentBalance(Account account, decimal balance)
        {
            Console.WriteLine($"Account {account.Address} balance: {balance}");
        }
    }
}
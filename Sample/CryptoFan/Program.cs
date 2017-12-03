using System;
using System.Threading;
using SecuredCommunication;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;
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
        static void Main(string[] args)
        {
            Console.WriteLine("Reciever - I just love getting new crypto coins");
            var password = "12345678";
            var account = Account.LoadFromKeyStore(File.ReadAllText(@"C:\temp\NetherumDemo\privchain\keystore\UTC--2017-11-30T13-36-01.594748200Z--863c813c74acee5e4063bd65e880c0f06d3cc765"), password);

            var kvInfo = new KeyVault("https://eladiw-testkv.vault.azure.net/");

            PrintCurrentBalance(account, EthereumWalletService.GetCurrentBalance(account.Address).Result);

            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new SecuredComm(secretsMgmnt, uri);

            var consumerTag =
                securedComm.ListenOnQueue("notifications",                                
                                          new string[] { "notifications.balance" },
                                          "signverify",
                                          (msg) =>
                                          {
                                            if (msg.data.Equals(account.Address, StringComparison.OrdinalIgnoreCase))
                                              {
                                                  Console.WriteLine("Great, Balance change!");
                                                  PrintCurrentBalance(account, EthereumWalletService.GetCurrentBalance(account.Address).Result);
                                              }
                                            else
                                              {
                                                  Console.WriteLine("Not my balance!");
                                                  Console.WriteLine(msg.data);
                                              }
                                          },
                                          "encdec");

            // wait 30 minutes
            Thread.Sleep(30 * 1000 * 60);

            securedComm.CancelListeningOnQueue(consumerTag);
        }

        public static void PrintCurrentBalance(Account account, decimal balance)
        {
            Console.WriteLine($"Account {account.Address} balance: {balance}");
        }
    }
}
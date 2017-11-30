using System;
using System.Threading;
using SecuredCommunication;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.IO;

namespace CryptoFan
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var password = "12345678";

            Console.WriteLine("I just love getting new crypto coins");
            var account = Account.LoadFromKeyStore(File.ReadAllText("somePath"), password);
            PrintCurrentBalance(account, web3);

            // TODO: write a sample app that registers on a queue, whenver it gets update
            //       it checks and prints the balance.

            var kvInfo = new KeyVaultInfo("https://eladiw-testkv.vault.azure.net/");
            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri("amqp://XXX:XXX@XXX:xx");
            var securedComm = new SecuredComm(secretsMgmnt, uri);

            var consumerTag =
                securedComm.ListenOnQueue("notifications",                                
                                          new string[] { "notifications.balance" },
                                          "signverify",
                                          (msg) =>
                                          {
                                            if (msg.data.Equals(account.Address))
                                              {
                                                  Console.WriteLine("Great, Balance change!");
                                                  PrintCurrentBalance(account, web3);
                                              }
                                            else
                                              {
                                                  Console.WriteLine("Not my balance!");
                                              }
                                          },
                                          "encdec");

            // wait 30 minutes
            Thread.Sleep(30 * 1000 * 60);

            securedComm.CancelListeningOnQueue(consumerTag);
        }

        public static void PrintCurrentBalance(Account account, Web3 web3)
        {
            var unitConverion = new Nethereum.Util.UnitConversion();
            var currentBalance = unitConverion.FromWei(web3.Eth.GetBalance.SendRequestAsync(account.Address).Result);
            Console.WriteLine($"Account {account.Address} balance: {currentBalance}");
        }
    }
}
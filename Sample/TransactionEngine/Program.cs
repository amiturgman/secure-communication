using System;
using SecuredCommunication;
using System.Configuration;
using System.Threading;

namespace TransactionEngine
{
    /// <summary>
    /// A sample app that listens for transactions requests.
    /// when one arrives, perform it, and notify about the change 
    /// to the fans
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("TransactionEngine - I do as I told");

            var unitConverion = new Nethereum.Util.UnitConversion();
            var service = new Nethereum.KeyStore.KeyStoreService();

            var kvInfo = new KeyVault("https://eladiw-testkv.vault.azure.net/");
            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new SecuredComm(secretsMgmnt, uri);
            var ethereumNodeWrapper = new EthereumNodeWrapper();

            var consumerTag =
                securedComm.ListenOnQueue("innerQueue",
                                          new string[] { "*.transactions" },
                                          "signverify",
                                          (msg) =>
                                          {
                                              Console.WriteLine("GOT WORK!");
                                              // todo: actually do work
                                              var msgArray = msg.data.Split(";");
                                              var amount = unitConverion.ToWei(msgArray[0]);
                                              var senderName = msgArray[1];
                                              var reciverAddress = msgArray[2];
                                              var secretManagement = new SecretsManagement(new KeyVault("https://eladiw-testkv.vault.azure.net/"));
                                              var ethereumWallet = new EthereumWalletService("https://eladiw-testkv.vault.azure.net/", secretManagement);

                                              try
                                              {
                                                  var transactionHash = ethereumWallet.SignTransaction("sender", reciverAddress, amount).Result;
                                                  var trnsactionResult = ethereumNodeWrapper.SendTransaction(transactionHash).Result;
                                              }
                                              catch (Exception ex)
                                              {
                                                  Console.WriteLine(ex.Message);
                                              }

                                              // Wait for miner
                                              Thread.Sleep(30000);

                                              securedComm.SendEncryptedMsgAsync(
                                              "encdec",
                                              "signverify",
                                              "notifications",
                                              "notifications.balance",
                                               new Message(reciverAddress)).Wait();
                                          });
        }
    }
}

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
            var secretsMgmnt = new SecretsManagement("enc", "dec", "sign", "verify", kvInfo, kvInfo);

            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new SecuredComm(secretsMgmnt, uri, "verify", "sign", false, "enc", "dec");
            var ethereumNodeWrapper = new EthereumNodeWrapper(kvInfo, secretsMgmnt);

            var consumerTag =
                securedComm.ListenOnQueue("transactions", new string[] { "*.transactions" },
                                          (msg) =>
                                          {
                                              Console.WriteLine("GOT WORK!");
                                              // todo: actually do work
                                              var msgArray = msg.data.Split(";");
                                              var amount = unitConverion.ToWei(msgArray[0]);
                                              var senderName = msgArray[1];
                                              var reciverAddress = msgArray[2];

                                              try
                                              {
                                                  var transactionHash = ethereumNodeWrapper.SignTransaction("sender", reciverAddress, amount).Result;
                                                  var trnsactionResult = ethereumNodeWrapper.SendTransaction(transactionHash).Result;
                                              }
                                              catch (Exception ex)
                                              {
                                                  Console.WriteLine(ex.Message);
                                              }

                                              // Wait for miner
                                              Thread.Sleep(30000);

                                              securedComm.SendMsgAsync(
                                              "notifications.balance",
                                               new Message(reciverAddress));
                                          });
        }
    }
}

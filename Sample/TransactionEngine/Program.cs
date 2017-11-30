using System;
using SecuredCommunication;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;

namespace TransactionEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I do as I told");
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var password = "12345678";
            var unitConverion = new Nethereum.Util.UnitConversion();
            var service = new Nethereum.KeyStore.KeyStoreService();

            // TODO: write a sample app that listens for transactions requests.
            //       when one arrives, perform it, and notify about the change 
            //       to the fans

            var kvInfo = new KeyVaultInfo("https://eladiw-testkv.vault.azure.net/");
            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri("amqp://XXX:XXX@XXX:xx");
            var securedComm = new SecuredComm(secretsMgmnt, uri);

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

                                              var privateKey = secretsMgmnt.GetPrivateKey(kvInfo.Url, "sender").Result;
                                              var senderAddress = secretsMgmnt.GetPublicKey(kvInfo.Url, "sender").Result;

                                              var txCount = web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderAddress).Result;
                                              var encoded = Web3.OfflineTransactionSigner.SignTransaction(privateKey, reciverAddress, amount, txCount.Value);
                                              var SendRawTransaction = web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(encoded).Result;
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

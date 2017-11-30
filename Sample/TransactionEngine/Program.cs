using System;
using SecuredCommunication;

namespace TransactionEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I do as I told");

            // TODO: write a sample app that listens for transactions requests.
            //       when one arrives, perform it, and notify about the change 
            //       to the fans

            var kvInfo = new KeyVaultInfo("", "", "");
            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri("amqp://XXX:XXX@XXX:xx");
            var securedComm = new SecuredComm(secretsMgmnt, uri);

            var consumerTag =
                securedComm.ListenOnQueue("innerQueue",
                                          "signverify",
                                          new string[] { "*.transactions" },
                                          (msg) =>
                                          {
                                              Console.WriteLine("GOT WORK!");
                                              // todo: actually do work

                                              securedComm.SendEncryptedMsgAsync(
                                              "encdec",
                                              "signverify",
                                              "notifications",
                                              "notifications.balance",
                                               new Message("0x12345")).Wait();
                                          });
        }
    }
}

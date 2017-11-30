using System;
using System.Threading;
using System.Threading.Tasks;
using SecuredCommunication;

namespace CryptoFan
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("I just love getting new crypto coins");
            Console.WriteLine("Current Balance is : ");

            // TODO: write a sample app that registers on a queue, whenver it gets update
            //       it checks and prints the balance.

            var kvInfo = new KeyVaultInfo("", "", "");
            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri("amqp://XXX:XXX@XXX:xx");
            var securedComm = new SecuredComm(secretsMgmnt, uri);

            var consumerTag =
                securedComm.ListenOnQueue("notifications",
                                          "signverify",
                                          new string[] { "notifications.balance" },
                                          (msg) =>
                                          {
                                            Console.WriteLine("Great, Balance change!");
                                            Console.WriteLine("New balance is: ");
                                          },
                                          "encdec");

            // wait 30 minutes
            Thread.Sleep(30 * 1000 * 60);

            securedComm.CancelListeningOnQueue(consumerTag);
        }
    }
}
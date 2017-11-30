using System;
using System.Threading;
using SecuredCommunication;

namespace cryptoPhilanthrop
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Happily to transfer my crypto coins!");
            Console.WriteLine("Current Balance is : ");

            // TODO: write a sample app that checks balance and while > some value
            //       keep asking the transferer to do more transactions

            var balance = 20000;// todo get from network

            var kvInfo = new KeyVaultInfo("", "", "");
            var secretsMgmnt = new SecretsManagement(kvInfo);

            var uri = new Uri("amqp://XXX:XXX@XXX:xx");
            var securedComm = new SecuredComm(secretsMgmnt, uri);

            while (balance > 10000)
            {
                // 
                securedComm.SendEncryptedMsgAsync(
                    "encdec",
                    "signverify",
                    "innerQueue",
                    "send.transactions",
                    new Message("1;0x12345")).Wait();

                balance = balance - 1;// todo, get from network
            }
        }
    }
}

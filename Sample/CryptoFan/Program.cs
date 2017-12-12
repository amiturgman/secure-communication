using System;
using System.Threading;
using SecuredCommunication;
using Contracts;

namespace CryptoFan
{
    /// <summary>
    /// A sample app that registers on a queue, whenver it gets update
    /// it checks and prints the balance.
    /// </summary>
    class Program
    {
        #region private members

        private const string c_keyVaultUri = "https://ilanasecurecommkv1012.vault.azure.net/";
        private const string c_encKeyName = "newname";
        private const string c_decKeyName = "newname";
        private const string c_signKeyName = "newname";
        private const string c_verifyKeyName = "newname";
        private const string c_ethereumTestNodeUrl = "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE";
        private const string c_ReciverId = "account2testnent";

        #endregion

        static void Main(string[] args)
        {
            // Init
            var kv = new KeyVault(c_keyVaultUri);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kv, c_ethereumTestNodeUrl);

            Console.WriteLine("Reciever - I just love getting new crypto coins");

            var reciverAddress = "0xEfD6AD01A596e0f56E8b3b19bFb636A0CC2af7ec"; //kvInfo.GetPublicKeyAsync(c_ReciverId).Result;            
            PrintCurrentBalance(reciverAddress, ethereumNodeWrapper.GetCurrentBalance(reciverAddress).Result);

            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kv, kv);
            //var securedComm = new RabbitMQBusImpl(secretsMgmnt, true, "securedCommExchange");
            var securedComm = new AzureQueueImpl(secretsMgmnt, true);

            // Listen on the notifications queue, check balance when a notification arrives
            var consumerTag =
                securedComm.Dequeue("notifications",
                    msg =>
                    {
                        var data = Utils.FromByteArray<string>(msg.Data);
                        if (data.Equals(reciverAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Great, Balance change!");
                            PrintCurrentBalance(reciverAddress, ethereumNodeWrapper.GetCurrentBalance(reciverAddress).Result);
                        }
                        else
                        {
                            Console.WriteLine("Not my balance!");
                            Console.WriteLine(msg.Data);
                        }
                    });

            // wait 30 minutes
            Thread.Sleep(30 * 1000 * 60);

            // switch based on the chosen queue
            //securedComm.CancelListeningOnQueue(consumerTag.Result);
            securedComm.CancelListeningOnQueue();
        }

        private static void PrintCurrentBalance(string address, decimal balance)
        {
            Console.WriteLine($"Account {address} balance: {balance}");
        }
    }
}
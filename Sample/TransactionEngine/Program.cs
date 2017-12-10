using System;
using SecuredCommunication;
using System.Threading;
using Contracts;

namespace TransactionEngine
{
    /// <summary>
    /// A sample app that listens for transactions requests.
    /// when one arrives, perform it, and notify about the change 
    /// to the fans
    /// </summary>
    class Program
    {
        #region private members

        private const string c_keyVaultUri = "https://eladiw-testkv.vault.azure.net/";
        private const string c_ethereumTestNodeUrl = "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE";
        private const string c_encKeyName = "demo-encryption";
        private const string c_decKeyName = "demo-encryption";
        private const string c_signKeyName = "demo-sign";
        private const string c_verifyKeyName = "demo-sign";

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("TransactionEngine - I do as I told");

            // Init
            var unitConverion = new Nethereum.Util.UnitConversion();

            var kvInfo = new KeyVault(c_keyVaultUri);
            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);

            //var securedComm = new RabbitMQBusImpl(secretsMgmnt, true, "securedCommExchange");
            var securedComm = new AzureQueueImpl(secretsMgmnt, true);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kvInfo, c_ethereumTestNodeUrl);

            // Listen on transactions requests, process them and notify the users when done
            securedComm.Dequeue("transactions",
                msg =>
                {
                    Console.WriteLine("Got work!");

                    var data = Utils.FromByteArray<string>(msg.Data);
                    var msgArray = data.Split(";");
                    var amount = unitConverion.ToWei(msgArray[0]);
                    var senderName = msgArray[1];
                    var reciverAddress = msgArray[2];

                    try
                    {
                        var transactionHash = ethereumNodeWrapper.SignTransaction(senderName, reciverAddress, amount).Result;
                        var transactionResult = ethereumNodeWrapper.SendTransaction(transactionHash).Result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        throw;
                    }

                    // Wait for miner
                    Thread.Sleep(30000);

                    // notify a user about his balance change
                    securedComm.EnqueueAsync("notifications", reciverAddress).Wait();
                }).Wait();
        }
    }
}

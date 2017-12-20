using System;
using System.Configuration;
using SecuredCommunication;
using System.Threading;
using System.Threading.Tasks;

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

            // Init
            var unitConverion = new Nethereum.Util.UnitConversion();

            var kv = new KeyVault(ConfigurationManager.AppSettings["AzureKeyVaultUri"],
                ConfigurationManager.AppSettings["applicationId"], ConfigurationManager.AppSettings["applicationSecret"]);

            var encryptionKeyName = ConfigurationManager.AppSettings["EncryptionKeyName"];
            var decryptionKeyName = ConfigurationManager.AppSettings["DecryptionKeyName"];
            var signKeyName = ConfigurationManager.AppSettings["SignKeyName"];
            var verifyKeyName = ConfigurationManager.AppSettings["VerifyKeyName"];

            var secretsMgmnt = new KeyVaultSecretManager(encryptionKeyName, decryptionKeyName, signKeyName, verifyKeyName, kv, kv);
            secretsMgmnt.Initialize().Wait();

            //var securedComm = new RabbitMQBusImpl(ConfigurationManager.AppSettings["rabbitMqUri"], secretsMgmnt, true, "securedCommExchange");
            var securedCommForTransactions = new AzureQueueImpl("transactions", ConfigurationManager.AppSettings["AzureStorageConnectionString"], secretsMgmnt, true);
            var securedCommForNotifications = new AzureQueueImpl("notifications", ConfigurationManager.AppSettings["AzureStorageConnectionString"], secretsMgmnt, true);
            var taskInitTransactions = securedCommForTransactions.Initialize();
            var taskInitNotifications = securedCommForNotifications.Initialize();
            Task.WhenAll(taskInitTransactions, taskInitNotifications).Wait();

            var ethereumNodeWrapper = new EthereumNodeWrapper(kv, ConfigurationManager.AppSettings["EthereumNodeUrl"]);

            // Listen on transactions requests, process them and notify the users when done
            securedCommForTransactions.DequeueAsync(
                msg =>
                {
                    Console.WriteLine("Got work!");

                    var data = Utils.FromByteArray<string>(msg);
                    var msgArray = data.Split(";");
                    var amount = unitConverion.ToWei(msgArray[0]);
                    var senderName = msgArray[1];
                    var reciverAddress = msgArray[2];

                    try
                    {
                        var transactionHash = ethereumNodeWrapper.SignTransactionAsync(senderName, reciverAddress, amount).Result;
                        var transactionResult = ethereumNodeWrapper.SendRawTransactionAsync(transactionHash).Result;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        throw;
                    }

                    // Wait for miner
                    Thread.Sleep(3000);

                    // notify a user about his balance change
                    securedCommForNotifications.EnqueueAsync(reciverAddress).Wait();
                },
                TimeSpan.FromSeconds(3)).Wait();
        }
    }
}

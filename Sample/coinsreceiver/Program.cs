﻿using System;
using System.Configuration;
using System.Threading;
using Contracts;
using SecuredCommunication;

namespace CoinsReceiver
{
    /// <summary>
    /// A sample app that registers on a queue, whenever it gets update
    /// it checks and prints the balance.
    /// </summary>
    class Program
    {
        #region private members


        private const string c_ReciverId = "reciverAccount";

        #endregion

        static void Main(string[] args)
        {
            // Init
            var kv = new KeyVault(ConfigurationManager.AppSettings["AzureKeyVaultUri"], 
                ConfigurationManager.AppSettings["applicationId"], ConfigurationManager.AppSettings["applicationSecret"]);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kv, ConfigurationManager.AppSettings["EthereumNodeUrl"]);

            Console.WriteLine("Receiver - I just love getting new crypto coins");

            var reciverAddress = ethereumNodeWrapper.GetPublicKeyAsync(c_ReciverId).Result;            
            PrintCurrentBalance(reciverAddress, ethereumNodeWrapper.GetCurrentBalance(reciverAddress).Result);

            var encryptionKeyName = ConfigurationManager.AppSettings["EncryptionKeyName"];
            var decryptionKeyName = ConfigurationManager.AppSettings["DecryptionKeyName"];
            var signKeyName = ConfigurationManager.AppSettings["SignKeyName"];
            var verifyKeyName = ConfigurationManager.AppSettings["VerifyKeyName"];

            var secretsMgmnt = new KeyVaultSecretManager(encryptionKeyName, decryptionKeyName, signKeyName, verifyKeyName, kv, kv);
            secretsMgmnt.Initialize().Wait();
            //var securedComm = new RabbitMQBusImpl(ConfigurationManager.AppSettings["rabbitMqUri"], secretsMgmnt, true, "securedCommExchange");
            var securedComm = new AzureQueueImpl("notifications", ConfigurationManager.AppSettings["AzureStorageConnectionString"], secretsMgmnt, true);
            securedComm.Initialize().Wait();

            // Listen on the notifications queue, check balance when a notification arrives
            var consumerTag =
                securedComm.DequeueAsync(
                    msg =>
                    {
                        var data = Utils.FromByteArray<string>(msg);
                        if (data.Equals(reciverAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Great, Balance change!");
                            PrintCurrentBalance(reciverAddress, ethereumNodeWrapper.GetCurrentBalance(reciverAddress).Result);
                        }
                        else
                        {
                            Console.WriteLine("Not my balance!");
                            Console.WriteLine(msg);
                        }
            }, TimeSpan.FromSeconds(3));

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
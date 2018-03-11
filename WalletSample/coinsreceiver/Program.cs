﻿using System;
using System.Configuration;
using System.Threading;
using Blockchain;
using Communication;
using Communication.AzureQueueDependencies;
using Cryptography;
using static Cryptography.KeyVaultCryptoActions;

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
                ConfigurationManager.AppSettings["applicationId"],
                ConfigurationManager.AppSettings["applicationSecret"]);
            var sqlDb = new SqlConnector(ConfigurationManager.AppSettings["SqlUserID"],
                ConfigurationManager.AppSettings["SqlPassword"],
                ConfigurationManager.AppSettings["SqlInitialCatalog"],
                ConfigurationManager.AppSettings["SqlDataSource"],
                ConfigurationManager.AppSettings["applicationId"],
                ConfigurationManager.AppSettings["applicationSecret"]);
            sqlDb.Initialize().Wait();

            var ethereumAccount = new EthereumAccount(sqlDb, ConfigurationManager.AppSettings["EthereumNodeUrl"]);

            Console.WriteLine("Receiver - I just love getting new crypto coins");

            var reciverAddress = ethereumAccount.GetPublicAddressAsync(c_ReciverId).Result;
            PrintCurrentBalance(reciverAddress, ethereumAccount.GetCurrentBalance(reciverAddress).Result);

            var encryptionKeyName = ConfigurationManager.AppSettings["EncryptionKeyName"];
            var decryptionKeyName = ConfigurationManager.AppSettings["DecryptionKeyName"];
            var signKeyName = ConfigurationManager.AppSettings["SignKeyName"];
            var verifyKeyName = ConfigurationManager.AppSettings["VerifyKeyName"];

            var encryptionCertPassword = ConfigurationManager.AppSettings["EncryptionCertPassword"];
            var decryptionCertPassword = ConfigurationManager.AppSettings["DecryptionCertPassword"];
            var signCertPassword = ConfigurationManager.AppSettings["SignCertPassword"];
            var verifyCertPassword = ConfigurationManager.AppSettings["VerifyCertPassword"];

            var secretsMgmnt =
                new KeyVaultCryptoActions(
                    new CertificateInfo(encryptionKeyName, encryptionCertPassword),
                    new CertificateInfo(decryptionKeyName, decryptionCertPassword),
                    new CertificateInfo(signKeyName, signCertPassword),
                    new CertificateInfo(verifyKeyName, verifyCertPassword),
                    kv,
                    kv);

            secretsMgmnt.InitializeAsync().Wait();
            //var securedComm = new RabbitMQBusImpl(ConfigurationManager.AppSettings["rabbitMqUri"], secretsMgmnt, true, "securedCommExchange");
            var queueClient =
                new CloudQueueClientWrapper(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            var securedComm = new AzureQueue("notifications", queueClient, secretsMgmnt, true);
            securedComm.Initialize().Wait();

            // Listen on the notifications queue, check balance when a notification arrives
            var consumerTag =
                securedComm.DequeueAsync(
                    msg =>
                    {
                       var data = Communication.Utils.FromByteArray<string>(msg);
                        if (data.Equals(reciverAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Great, Balance change!");
                            PrintCurrentBalance(reciverAddress,
                                ethereumAccount.GetCurrentBalance(reciverAddress).Result);
                        }
                        else
                        {
                            Console.WriteLine("Not my balance!");
                            Console.WriteLine(msg);
                        }
                    }, (message) => { Console.WriteLine("Verification failure, doing nothing"); },
                    TimeSpan.FromSeconds(3));

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
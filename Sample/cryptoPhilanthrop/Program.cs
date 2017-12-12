﻿using System;
using System.Threading;
using SecuredCommunication;

namespace cryptoPhilanthrop
{
    /// <summary>
    ///  A sample app that checks balance and while > some value
    ///  keep asking the transferer to do more transactions
    /// </summary>
    class Program
    {
        #region private members

        private const string c_keyVaultUri = "https://ilanasecurecommkv.vault.azure.net/";
        private const string c_ethereumTestNodeUrl = "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE";
        private const string c_encKeyName = "testser";
        private const string c_decKeyName = "testser";
        private const string c_signKeyName = "testser";
        private const string c_verifyKeyName = "testser";
        private const string c_senderId = "account1testnent";
        private const string c_ReciverId = "account2testnent";

        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Sender - Happy to transfer my crypto coins!");

            // Init
            var kvInfo = new KeyVault(c_keyVaultUri);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kvInfo, c_ethereumTestNodeUrl);

            var senderAddress = "0xF0b5364cA485fF5fBBcC301b9Ad09F8B91715867"; //kvInfo.GetPublicKeyAsync(c_senderId).Result;
            var reciverAddress = "0xEfD6AD01A596e0f56E8b3b19bFb636A0CC2af7ec"; //kvInfo.GetPublicKeyAsync(c_ReciverId).Result;
            var balance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
            PrintCurrentBalance(senderAddress, balance);

            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            //var securedComm = new RabbitMQBusImpl(secretsMgmnt, true, "securedCommExchange");
            var securedComm = new AzureQueueImpl(secretsMgmnt, true);

            // While there are sufficient funds, transfer some...
            while (balance > 0)
            {
                var amountToSend = 0.001;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                securedComm.EnqueueAsync(
                    "transactions",
                    $"{amountToSend};{c_senderId};{reciverAddress}").Wait();

                // Sleep 10 minutes
                Thread.Sleep(600000);

                var newBalance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
                PrintCurrentBalance(senderAddress, newBalance);

                // Wait for mining.. 
                while (newBalance.Equals(balance))
                {
                    newBalance = ethereumNodeWrapper.GetCurrentBalance(senderAddress).Result;
                }

                balance = newBalance;
            }
        }

        public static void PrintCurrentBalance(string address, decimal balance)
        {
            Console.WriteLine($"Account {address} balance: {balance}");
        }
    }
}

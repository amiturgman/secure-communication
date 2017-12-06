﻿using System;
using System.Threading;
using SecuredCommunication;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Configuration;

namespace CryptoFan
{
    /// <summary>
    /// A sample app that registers on a queue, whenver it gets update
    /// it checks and prints the balance.
    /// </summary>
    class Program
    {
        #region private members
        
        private const string c_keyVaultUri = "https://eladiw-testkv.vault.azure.net/";
        private const string c_encKeyName = "enc_public";
        private const string c_decKeyName = "dec_private";
        private const string c_signKeyName = "sign_private";
        private const string c_verifyKeyName = "verify_public";
        private const string c_ethereumTestNodeUrl = "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE";
        private const string c_ReciverId = "account2testnent";

        #endregion

        static void Main(string[] args)
        {
            // Init
            var kv = new KeyVault(c_keyVaultUri);
            var ethereumNodeWrapper = new EthereumNodeWrapper(kv, c_ethereumTestNodeUrl);

            Console.WriteLine("Reciever - I just love getting new crypto coins");

            var reciverAddress = kv.GetPublicKeyAsync(c_ReciverId).Result;
            PrintCurrentBalance(reciverAddress, ethereumNodeWrapper.GetCurrentBalance(reciverAddress).Result);

            var secretsMgmnt = new SecretsManagement(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kv, kv);
            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new RabbitMQBusImpl(secretsMgmnt, uri, true);

            // Listen on the notifications queue, check balance when a notification arrives
            var consumerTag =
                securedComm.Dequeue("notifications",
                                          msg =>
                                          {
                                              if (msg.Data.Equals(reciverAddress, StringComparison.OrdinalIgnoreCase))
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

            securedComm.CancelListeningOnQueue(consumerTag);
        }

        private static void PrintCurrentBalance(string address, decimal balance)
        {
            Console.WriteLine($"Account {address} balance: {balance}");
        }
    }
}
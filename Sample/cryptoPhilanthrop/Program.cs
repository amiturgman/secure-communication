﻿using System;
using System.Threading;
using SecuredCommunication;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Configuration;

namespace cryptoPhilanthrop
{
    /// <summary>
    ///  A sample app that checks balance and while > some value
    ///  keep asking the transferer to do more transactions
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var password = "12345678";

            Console.WriteLine("Sender - Happy to transfer my crypto coins!");
            var account = Account.LoadFromKeyStore(File.ReadAllText(@"C:\temp\NetherumDemo\privchain\keystore\UTC--2017-11-30T13-34-42.742317500Z--bb6d204b166279511ce6cb4547275e805bc8cb82"), password);

            var balance = EthereumNodeWrapper.GetCurrentBalance(account.Address).Result;
            PrintCurrentBalance(account, balance);
            var newBalance = balance;

            var kvInfo = new KeyVault("https://eladiw-testkv.vault.azure.net/");
            var secretsMgmnt = new SecretsManagement("enc", "dec", "sign", "verify", kvInfo, kvInfo);

            var uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"]);
            var securedComm = new SecuredComm(secretsMgmnt, uri);

            while (balance > 10000)
            {
                var amountToSend = 5000;
                // Message structure: {amountToSend};{senderName};{reciverAddress}
                securedComm.SendEncryptedMsgAsync(
                    "encdec",
                    "signverify",
                    "innerQueue",
                    "send.transactions",
                    new Message($"{amountToSend};sender;0x863c813c74acee5e4063bd65e880c0f06d3cc765")).Wait();

                Thread.Sleep(60000);
                
                newBalance = EthereumNodeWrapper.GetCurrentBalance(account.Address).Result;
                PrintCurrentBalance(account, newBalance);

                // Wait for mining.. 
                while (newBalance.Equals(balance))
                {
                    newBalance = EthereumNodeWrapper.GetCurrentBalance(account.Address).Result;
                }

                balance = newBalance;
                Console.WriteLine(balance - newBalance);
            }
        }

        public static void PrintCurrentBalance(Account account, decimal balance)
        {
            Console.WriteLine($"Account {account.Address} balance: {balance}");
        }
    }
}

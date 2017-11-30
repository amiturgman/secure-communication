using System;
using System.IO;
using Nethereum.Geth;
using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Threading;

namespace EthereumTest
{
    class Program
    {
    
        static void Main(string[] args)
        {
            // create account 
            var password = "12345678";
            var path = @"C:\temp\NetherumDemo\test\keystore";

            // Initialize
            var service = new Nethereum.KeyStore.KeyStoreService();
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var web3Geth = new Web3Geth(client);
            var unitConverion = new Nethereum.Util.UnitConversion();

            //// Create Accounts
            //var accountOnePath = Directory.GetFiles(path)[0];
            //var accountTwoPath = Directory.GetFiles(path)[1];
            var accountTwoPath = Path.Combine(path, CreateAccount(password, path));

           // //var account1 = Account.LoadFromKeyStore(File.ReadAllText(accountOnePath), password);
           // var account2 = Account.LoadFromKeyStore(File.ReadAllText(accountTwoPath), password);

           // PrintCurrentBalance(account1, web3);
           // PrintCurrentBalance(account2, web3);

           // var amount = unitConverion.ToWei(5000);
           // var privateKey = service.DecryptKeyStoreFromJson(password, File.ReadAllText(accountOnePath)).ToHex(true);

           // var txCount = web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account1.Address).Result;
           // var encoded = Web3.OfflineTransactionSigner.SignTransaction(privateKey, account2.Address, amount, txCount.Value);
           // var SendRawTransaction = web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(encoded).Result;

           // Console.WriteLine($"Sending Transaction....");
           // Console.WriteLine();
           // Console.WriteLine();

           // var transactionResult = web3Geth.Miner.Start.SendRequestAsync(100).Result;
           // Thread.Sleep(6000);
           // var minerStop = web3Geth.Miner.Stop.SendRequestAsync(100).Result;
           // Thread.Sleep(3000);

           // PrintCurrentBalance(account1, web3);
           // PrintCurrentBalance(account2, web3);
           // Console.WriteLine("finish");
           // Console.ReadLine();
        }

        public static void PrintCurrentBalance(Account account, Web3 web3)
        {
            var unitConverion = new Nethereum.Util.UnitConversion();
            var currentBalance = unitConverion.FromWei(web3.Eth.GetBalance.SendRequestAsync(account.Address).Result);
            Console.WriteLine($"Account {account.Address} balance: {currentBalance}");
        }

        public static string CreateAccount(string password, string path)
        {
            //Generate a private key pair using SecureRandom
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            //Get the public address (derivied from the public key)
            var address = ecKey.GetPublicAddress();

            //Create a store service, to encrypt and save the file using the web3 standard
            var service = new Nethereum.KeyStore.KeyStoreService();
            var encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address);
            var fileName = service.GenerateUTCFileName(address);
            //save the File
            using (var newfile = File.CreateText(Path.Combine(path, fileName)))
            {
                newfile.Write(encryptedKey);
                newfile.Flush();
            }

            return fileName;
        }
    }
}

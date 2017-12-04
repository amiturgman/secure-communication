using Nethereum.JsonRpc.IpcClient;
using Nethereum.Signer;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    /// <summary>
    /// Class for accessing the Ethereum node.
    /// </summary>
    public class EthereumNodeWrapper : IBlockchainNodeWrapper
    {
        private ISecretsManagement secretsManagement;
        private string keyVaultUrl;

        #region Public Methods
        public EthereumNodeWrapper(string keyVaultUrl, ISecretsManagement secretsManagement)
        {
            this.keyVaultUrl = keyVaultUrl;
            this.secretsManagement = secretsManagement;
        }

        /// <summary>
        /// Creates blockchain account and store the public and private keys in Azure KeyVault 
        /// </summary>
        /// <returns>The public private key vault</returns>
        public async Task<KeyPair> GenerateAccount()
        {
            var ecKey = EthECKey.GenerateKey();

            return await Task.FromResult(new KeyPair(ecKey.GetPublicAddress(), ecKey.GetPrivateKey()));
        }

        /// <summary>
        /// Send the transaction to the public node. 
        /// </summary>
        /// <param name="transactionHash">The transaction hash</param>
        /// <returns>The transaction result</returns>
        public async Task<string> SendTransaction(string hash)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var transactionResult = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(hash);
            return transactionResult;
        }

        /// <summary>
        /// Sign a blockchain transaction
        /// </summary>
        /// <param name="senderIdentifier">The sender identifier (Id, name etc.)</param>
        /// <param name="recieverAddress">The reciver address</param>
        /// <param name="amountInWei">The amount to send in wei (ethereum units)</param>
        /// <returns>The transaction hash</returns>
        public async Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amountInWei)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);

            var senderKeyPair = await LoadKeyPairFromKeyVault(senderIdentifier);
            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderKeyPair.PublicKey);
            var transactionHash = Web3.OfflineTransactionSigner.SignTransaction(senderKeyPair.PrivateKey, recieverAddress, amountInWei, txCount.Value);

            return await Task.FromResult(transactionHash);
        }

        /// <summary>
        /// Gets the balance of the provided account
        /// </summary>
        /// <param name="address">The public address of the acocount</param>
        /// <returns>Returns the balance in ether.</returns>
        public static async Task<decimal> GetCurrentBalance(string address)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var unitConverion = new Nethereum.Util.UnitConversion();
            var currentBalance = unitConverion.FromWei(await web3.Eth.GetBalance.SendRequestAsync(address));
            return currentBalance;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Loads the identifier's public private keys from the Key Vault
        /// </summary>
        /// <param name="identifier">The identifier name/id</param>
        /// <returns>The public private key pair</returns>
        private async Task<KeyPair> LoadKeyPairFromKeyVault(string identifier)
        {
            var publicKey = await secretsManagement.GetPublicKey(keyVaultUrl, identifier);
            var privateKey = await secretsManagement.GetPrivateKey(keyVaultUrl, identifier);

            return new KeyPair(publicKey, privateKey);
        }

        #endregion
    }
}

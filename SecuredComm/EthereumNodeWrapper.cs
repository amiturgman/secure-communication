using System;
using Nethereum.Signer;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;
using Contracts;

namespace SecuredCommunication
{
    /// <summary>
    /// Class for accessing the Ethereum node.
    /// </summary>
    public class EthereumNodeWrapper : IBlockchainNodeWrapper
    {
        private Web3 web3;
        private IKeyVault m_kv;
        private const string publicKeySuffix = "-public";
        private const string privateKeySuffix = "-private";

        #region Public Methods
        public EthereumNodeWrapper(IKeyVault keyVault, string nodeUrl = "")
        {
            m_kv = keyVault;
            web3 = string.IsNullOrEmpty(nodeUrl) ? new Web3() : new Web3(nodeUrl);
        }

        /// <summary>
        /// Creates blockchain account and store the public and private keys in Azure KeyVault 
        /// </summary>
        /// <returns>The public private key vault</returns>
        public KeyPair CreateAccount()
        {
            var ecKey = EthECKey.GenerateKey();

            return new KeyPair(ecKey.GetPublicAddress(), ecKey.GetPrivateKey());
        }

        /// <summary>
        /// Stores the account async.
        /// </summary>
        /// <returns>The account async.</returns>
        /// <param name="identifier">Identifier.</param>
        /// <param name="key">Key.</param>
        public async Task<bool> StoreAccountAsync(string identifier, KeyPair key)
        {
            try
            {
                await m_kv.SetSecretAsync(identifier + publicKeySuffix, key.PublicKey);
                await m_kv.SetSecretAsync(identifier + privateKeySuffix, key.PrivateKey);
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }
        }

        /// <summary>
        /// Returns the private key by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's public key</returns>
        public async Task<string> GetPrivateKeyAsync(string identifier)
        {
            var secret = await m_kv.GetSecretAsync(identifier + privateKeySuffix);
            return secret.Value;
        }

        /// <summary>
        /// Returns the public key by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's public key</returns>
        public async Task<string> GetPublicKeyAsync(string identifier)
        {
            var secret = await m_kv.GetSecretAsync(identifier + publicKeySuffix);
            return secret.Value;
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
            var senderKeyPair = await LoadKeyPairFromKeyVault(senderIdentifier);
            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderKeyPair.PublicKey);
            return Web3.OfflineTransactionSigner.SignTransaction(senderKeyPair.PrivateKey, recieverAddress, amountInWei, txCount.Value);
        }

        /// <summary>
        /// Send the transaction to the public node. 
        /// </summary>
        /// <param name="hash">The transaction hash</param>
        /// <returns>The transaction result</returns>
        public async Task<string> SendRawTransaction(string hash)
        {
            return await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(hash);
        }

        /// <summary>
        /// Gets the balance of the provided account
        /// </summary>
        /// <param name="address">The public address of the acocount</param>
        /// <returns>Returns the balance in ether.</returns>
        public async Task<decimal> GetCurrentBalance(string address)
        {
            var unitConverion = new Nethereum.Util.UnitConversion();
            return unitConverion.FromWei(await web3.Eth.GetBalance.SendRequestAsync(address));
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
            var publicKey = await m_kv.GetSecretAsync(string.Concat(identifier, publicKeySuffix));
            var privateKey = await m_kv.GetSecretAsync(string.Concat(identifier, privateKeySuffix));

            return new KeyPair(publicKey.Value, privateKey.Value);
        }

        #endregion
    }
}

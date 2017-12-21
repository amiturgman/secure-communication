using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace Contracts
{
    /// <summary>
    /// Ethereum node wrapper - allows for operations (e.g. send transaction) against the Ethereum networks (public / local / tests)
    /// </summary>
    public interface IEthereumNodeWrapper
    {
        /// <summary>
        /// Creates blockchain account and store the public and private keys in Azure KeyVault 
        /// </summary>
        /// <returns>The public private key pair</returns>
        EthECKey CreateAccount();

        /// <summary>
        /// Stores a key pair into the Azure KeyVault.
        /// </summary>
        /// <returns>The created key pair.</returns>
        /// <param name="identifier">key pair identifier.</param>
        /// <param name="privateKey">The actual private key.</param>
        Task<bool> StoreAccountAsync(string identifier, string privateKey);

        /// <summary>
        /// Signs a blockchain transaction
        /// </summary>
        /// <param name="senderIdentifier">The sender identifier (Id, name, etc...)</param>
        /// <param name="recieverAddress">The receiver address</param>
        /// <param name="amountInWei">The amount to send</param>
        /// <returns>The signed transaction</returns>
        Task<string> SignTransactionAsync(string senderIdentifier, string recieverAddress, BigInteger amountInWei);

        /// <summary>
        /// Send the raw transaction to the public node. 
        /// </summary>
        /// <param name="signedTransaction">The transaction signed transaction</param>
        /// <returns>The transaction result</returns>
        Task<string> SendRawTransactionAsync(string signedTransaction);

        /// <summary>
        /// Returns the private key by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's private key</returns>
        Task<string> GetPrivateKeyAsync(string identifier);

        /// <summary>
        /// Returns the public key by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's public key</returns>
        Task<byte[]> GetPublicKeyAsync(string identifier);

        /// <summary>
        /// Returns the public address by the key vault identifier
        /// </summary>
        /// <param name="identifier">The user id</param>
        /// <returns>The user's public key</returns>
        Task<string> GetPublicAddressAsync(string identifier);
    }
}
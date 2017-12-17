using System.Numerics;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Blockchain node wrapper - allows for operations against the blockchain network (e.g. send transaction)
    /// </summary>
    public interface IBlockchainNodeWrapper
    {
        /// <summary>
        /// Creates blockchain account and store the public and private keys in Azure KeyVault 
        /// </summary>
        /// <returns>The public private key pair</returns>
        KeyPair CreateAccount();

        /// <summary>
        /// Stores a key pair into the Azure KeyVault.
        /// </summary>
        /// <returns>The created key pair.</returns>
        /// <param name="identifier">key pair identifier.</param>
        /// <param name="key">The actual key pair.</param>
        Task<bool> StoreAccountAsync(string identifier, KeyPair key);

        /// <summary>
        /// Signs a blockchain transaction
        /// </summary>
        /// <param name="senderIdentifier">The sender identifier (Id, name, etc...)</param>
        /// <param name="recieverAddress">The reciver address</param>
        /// <param name="amount">The amount to send</param>
        /// <returns>The transaction hash</returns>
        Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount);

        /// <summary>
        /// Send the raw transaction to the public node. 
        /// </summary>
        /// <param name="transactionHash">The transaction hash</param>
        /// <returns>The transaction result</returns>
        Task<string> SendRawTransaction(string transactionHash);

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
        Task<string> GetPublicKeyAsync(string identifier);
    }
}
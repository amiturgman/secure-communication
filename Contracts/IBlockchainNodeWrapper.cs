using System.Numerics;
using System.Threading.Tasks;

public interface IBlockchainNodeWrapper
{
    /// <summary>
    /// Creates blockchain account and store the public and private keys in Azure KeyVault 
    /// </summary>
    /// <returns>The public private key vault</returns>
    Task<KeyPair> GenerateAccount();

    /// <summary>
    /// Sign a blockchain transaction
    /// </summary>
    /// <param name="senderIdentifier">The sender identifier (Id, name etc.)</param>
    /// <param name="recieverAddress">The reciver address</param>
    /// <param name="amount">The amount to send</param>
    /// <returns>The transaction hash</returns>
    Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount);

    /// <summary>
    /// Send the transaction to the public node. 
    /// </summary>
    /// <param name="transactionHash">The transaction hash</param>
    /// <returns>The transaction result</returns>
    Task<string> SendTransaction(string transactionHash);
}
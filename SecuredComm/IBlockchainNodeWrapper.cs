using System.Numerics;
using System.Threading.Tasks;

public class KeyPair {
    public string privateKey;
    public string publicKey;
}

public interface IBlockchainNodeWrapper
{
    // Create account
    Task<KeyPair> GenerateAccount();

    // Sign Transaction
    Task<bool> SignTransaction(string privateKey, string recieverAddress, BigInteger amount, BigInteger nonce);

    // send raw transaction
    Task<bool> SendTransaction(string hash);
}
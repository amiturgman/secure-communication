using System.Numerics;
using System.Threading.Tasks;

public class KeyPair {
    public string PrivateKey { get; set; }
    public string PublicKey{ get; set; }

    public KeyPair(string publicKey, string privateKey)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
    }
}

public interface IBlockchainNodeWrapper
{
    // Create account
    Task<KeyPair> GenerateAccount();

    // send raw transaction
    Task<bool> SendTransaction(string hash);
}
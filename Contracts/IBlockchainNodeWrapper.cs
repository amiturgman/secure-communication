using System.Threading.Tasks;

public interface IBlockchainNodeWrapper
{
    // Create account
    Task<KeyPair> GenerateAccount();

    // send raw transaction
    Task<string> SendTransaction(string hash);
}
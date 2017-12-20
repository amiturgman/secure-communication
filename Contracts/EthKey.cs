namespace Contracts
{
    /// <summary>
    /// Holds the Ethereum public and private keys and public address
    /// </summary>
    public class EthKey : KeyPair
    {
        public string PublicAddress { get; }

        public EthKey(string privateKey, byte[] publicKey, string publicAddress) 
            : base(publicKey, privateKey)
        {
            PublicAddress = publicAddress;
        }
    }
}

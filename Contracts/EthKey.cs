namespace Contracts
{
    /// <summary>
    /// Holds the Ethereum keys + address
    /// </summary>
    public class EthKey
    {
        public KeyPair Pair { get; private set; }
        public string PublicAddress { get; private set; }

        public EthKey(string privateKey, byte[] publicKey, string publicAddress)
        {
            Pair = new KeyPair(publicKey, privateKey);
            PublicAddress = publicAddress;
        }
    }
}

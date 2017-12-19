namespace Contracts
{
    /// <summary>
    /// Simple wrapper for a public-private key pair
    /// </summary>
    public class KeyPair
    {
        public string PrivateKey { get; private set; }
        public byte[] PublicKey { get; private set; }

        public KeyPair(byte[] publicKey, string privateKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }
    }
}


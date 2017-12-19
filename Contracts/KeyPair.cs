namespace Contracts
{
    /// <summary>
    /// Simple wrapper for a public-private key pair
    /// </summary>
    public class KeyPair
    {
        public string PrivateKey { get; }
        public byte[] PublicKey { get; }

        public KeyPair(byte[] publicKey, string privateKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }
    }
}


namespace Contracts
{
    public class KeyPair
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }

        public KeyPair(string publicKey, string privateKey)
        {
            PrivateKey = privateKey;
            PublicKey = publicKey;
        }
    }
}


using System.Threading.Tasks;

namespace SecuredCommunication
{
    public interface ISecretsManagement
    {
        Task<string> Encrypt(string keyVaultUrl, string keyName, string data);

        Task<string> Decrypt(string keyVaultUrl, string keyName, string encryptedData);

        Task<byte[]> Sign(string keyVaultUrl, string keyName, string data);

        Task<bool> Verify(string keyVaultUrl, string keyName, byte[] signature, string data);

        Task<bool> StoreKeyPair(string keyVaultUrl, string identifier, KeyPair key);

        Task<string> GetPublicKey(string keyVaultUrl, string identifier);

        Task<string> GetPrivateKey(string keyVaultUrl, string identifier);
    }
}

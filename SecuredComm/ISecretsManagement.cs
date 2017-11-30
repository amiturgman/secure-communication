using System.Threading.Tasks;

namespace SecuredCommunication
{
    public interface ISecretsManagement
    {
        Task<string> Encrypt(string keyVaultName, string keyName, string data);

        Task<string> Decrypt(string keyVaultName, string keyName, string encryptedData);

        Task<string> Sign(string keyVaultName, string keyName, string data);

        Task<bool> Verify(string keyVaultName, string keyName, string signature);

        Task<bool> StoreKeyPair(string keyVaultName, string identifier, KeyPair key);

        Task<string> GeyPublicKey(string keyVaultName, string identifier);

        Task<string> GeyPrivateKey(string keyVaultName, string identifier);
    }
}

using System;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public interface ISecretsManagement
    {
        Task<string> Encrypt(string keyName, string data);

        Task<string> Decrypt(string keyName, string encryptedData);

        Task<string> Sign(string keyName, string data);

        Task<bool> Verify(string keyName, string signature);

        Task<bool> StoreKeyPair(string identifier, KeyPair key);

        Task<string> GeyPublicKey(string identifier);

        Task<string> GeyPrivateKey(string identifier);
    }
}

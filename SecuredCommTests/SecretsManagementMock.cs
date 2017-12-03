using System;
using System.Threading.Tasks;

namespace SecuredCommTests
{
    public class SecretsManagementMock: ISecretsManagement
    {
        public SecretsManagementMock()
        {
        }

        Task<string> ISecretsManagement.Decrypt(string keyVaultUrl, string keyName, string encryptedData)
        {
            throw new NotImplementedException();
        }

        Task<string> ISecretsManagement.Encrypt(string keyVaultUrl, string keyName, string data)
        {
            throw new NotImplementedException();
        }

        Task<string> ISecretsManagement.GetPrivateKey(string keyVaultUrl, string identifier)
        {
            throw new NotImplementedException();
        }

        Task<string> ISecretsManagement.GetPublicKey(string keyVaultUrl, string identifier)
        {
            throw new NotImplementedException();
        }

        Task<byte[]> ISecretsManagement.Sign(string keyVaultUrl, string keyName, string data)
        {
            throw new NotImplementedException();
        }

        Task<bool> ISecretsManagement.StoreKeyPair(string keyVaultUrl, string identifier, KeyPair key)
        {
            throw new NotImplementedException();
        }

        Task<bool> ISecretsManagement.Verify(string keyVaultUrl, string keyName, byte[] signature, string data)
        {
            throw new NotImplementedException();
        }
    }
}

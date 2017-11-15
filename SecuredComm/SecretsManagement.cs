using System;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class SecretsManagement : ISecretsManagement
    {
        public SecretsManagement(string kvName, string appId, string servicePrincipal)
        {
        }

        public async Task<string> Decrypt(string keyName, string encryptedData)
        {
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Encrypt(string keyName, string data)
        {
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Sign(string keyName, string data)
        {
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Verify(string keyName, string signature)
        {
            return await Task.FromResult("Not implemented");
        }
    }
}

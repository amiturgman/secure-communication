using System;
using System.Threading.Tasks;

namespace SecuredComm
{
    public class SecretsManagement : ISecretsManagement
    {
        public SecretsManagement(string kvName, string appId, string servicePrincipal)
        {
        }

        public async Task<string> Decrypt(string keyName, string encData)
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

        public async Task<string> Verify()
        {
            return await Task.FromResult("Not implemented");
        }
    }
}

using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
using Contracts;

namespace SecuredCommunication
{
    public class KeyVault : IKeyVault
    {
        public KeyVaultClient client;
        private readonly string _url;
        private readonly string _applicationId;
        private readonly string _applicationSecret;

        public KeyVault(string kvUrl, string applicationId, string applicationSecret)
        {
            _url = kvUrl;
            client = new KeyVaultClient(GetAccessTokenAsync, new HttpClient());
            _applicationId = applicationId;
            _applicationSecret = applicationSecret;
        }

        /// <summary>
        /// Get the Azure Key Vault Url
        /// </summary>
        /// <returns>The KeyVault Url</returns>
        public string GetUrl()
        {
            return _url;
        }

        public Task<SecretBundle> GetSecretAsync(string secretName) 
        { 
            return client.GetSecretAsync(GetUrl(), secretName);
        } 

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            return client.SetSecretAsync(GetUrl(), secretName, value);
        }

        public Task<KeyBundle> GetKeyAsync(string keyName, string keyVersion = null)
        {
            return keyVersion == null ? client.GetKeyAsync(GetUrl(), keyName) : client.GetKeyAsync(GetUrl(), keyName, keyVersion);
        }

        public Task<KeyOperationResult> EncryptAsync(string keyIdentifier, string algorithm, byte[] value)
        {
            return client.EncryptAsync(keyIdentifier, algorithm, value);
        }

        public async Task<KeyOperationResult> DecryptAsync(string keyIdentifier, string algorithm, byte[] value)
        {
            return await client.DecryptAsync(keyIdentifier, algorithm, value);
        }

        public Task<KeyOperationResult> SignAsync(string keyIdentifier, string algorithm, byte[] digest)
        {
            return client.SignAsync(keyIdentifier, algorithm, digest);
        }

        public Task<bool> VerifyAsync(string keyIdentifier, string algorithm, byte[] digest, byte[] signature)
        {
            return client.VerifyAsync(keyIdentifier, algorithm, digest, signature);
        }

#region Private Methods
        private async Task<string> GetAccessTokenAsync(
          string authority,
          string resource,
          string scope)
        {
            var clientCredential = new ClientCredential(_applicationId, _applicationSecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, clientCredential);

            return result.AccessToken;
        }
#endregion
    }
}

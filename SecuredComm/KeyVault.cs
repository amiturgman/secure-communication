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

        public KeyVault(string kvUrl)
        {
            _url = kvUrl;
            client = new KeyVaultClient(GetAccessTokenAsync, new HttpClient());
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
        private static async Task<string> GetAccessTokenAsync(
          string authority,
          string resource,
          string scope)
        {
            //clientID and clientSecret are obtained by registering
            //the application in Azure AD
            var clientId = ConfigurationManager.AppSettings["applicationId"];
            var clientSecret = ConfigurationManager.AppSettings["applicationSecret"];

            var clientCredential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, clientCredential);

            return result.AccessToken;
        }
#endregion
    }
}

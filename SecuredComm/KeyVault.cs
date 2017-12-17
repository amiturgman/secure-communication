using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
using Contracts;

namespace SecuredCommunication
{
    public class KeyVault : IKeyVault
    {
        public KeyVaultClient m_kvClient;

        #region private members

        private readonly string m_url;
        private readonly string m_applicationId;
        private readonly string m_applicationSecret;

        #endregion

        public KeyVault(string kvUrl, string applicationId, string applicationSecret)
        {
            m_url = kvUrl;
            m_kvClient = new KeyVaultClient(GetAccessTokenAsync, new HttpClient());
            m_applicationId = applicationId;
            m_applicationSecret = applicationSecret;
        }

        /// <summary>
        /// Get the Azure Key Vault Url
        /// </summary>
        /// <returns>The KeyVault Url</returns>
        public string GetUrl()
        {
            return m_url;
        }

        public Task<SecretBundle> GetSecretAsync(string secretName) 
        { 
            return m_kvClient.GetSecretAsync(GetUrl(), secretName);
        } 

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            return m_kvClient.SetSecretAsync(GetUrl(), secretName, value);
        }
       
        #region Private Methods

        private async Task<string> GetAccessTokenAsync(
          string authority,
          string resource,
          string scope)
        {
            var clientCredential = new ClientCredential(m_applicationId, m_applicationSecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, clientCredential);

            return result.AccessToken;
        }

        #endregion
    }
}

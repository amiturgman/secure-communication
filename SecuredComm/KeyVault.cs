using System;
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
        #region private members

        private KeyVaultClient m_kvClient;
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

        public async Task<SecretBundle> GetSecretAsync(string secretName)
        {
            try
            {
                return await m_kvClient.GetSecretAsync(GetUrl(), secretName);
            }
            catch (KeyVaultErrorException ex)
            {
                Console.WriteLine($"Exception while trying to get secret {secretName}, {ex.Message}");
                throw;
            }
        }

        public async Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            try
            {
                return await m_kvClient.SetSecretAsync(GetUrl(), secretName, value);
            }
            catch (KeyVaultErrorException ex)
            {
                Console.WriteLine($"Exception while trying to set secret {secretName}, {ex.Message}");
                throw;
            }
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

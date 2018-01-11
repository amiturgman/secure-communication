using System;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

namespace Cryptography
{
    public class KeyVault : IKeyVault
    {
        public string Url { get; }

        #region private members

        private KeyVaultClient m_kvClient;
        private readonly string m_applicationId;
        private readonly string m_applicationSecret;

        #endregion

        /// <summary>
        /// Ctor for Key vault class
        /// </summary>
        /// <param name="kvUrl">The Azure keyvault url</param>
        /// <param name="applicationId">The azure service principal application id</param>
        /// <param name="applicationSecret">The azure service principal application secret</param>
        public KeyVault(string kvUrl, string applicationId, string applicationSecret)
        {
            Url = kvUrl;
            m_applicationId = applicationId;
            m_applicationSecret = applicationSecret;
           
            m_kvClient = new KeyVaultClient(GetAccessTokenAsync, new HttpClient());
        }

        /// <summary>
        /// Gets the specified secret
        /// </summary>
        /// <returns>The secret</returns>
        /// <param name="secretName">Secret identifier</param>
        public async Task<SecretBundle> GetSecretAsync(string secretName)
        {
            try
            {
                return await m_kvClient.GetSecretAsync(Url, secretName);
            }
            catch (KeyVaultErrorException ex)
            {
                Console.WriteLine($"Exception while trying to get secret {secretName}, {ex}");
                throw;
            }
        }

        /// <summary>
        /// Sets a secret in Azure keyvault
        /// </summary>
        /// <returns>The secret.</returns>
        /// <param name="secretName">Secret identifier.</param>
        /// <param name="value">The value to be stored.</param>
        public async Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            try
            {
                return await m_kvClient.SetSecretAsync(Url, secretName, value);
            }
            catch (KeyVaultErrorException ex)
            {
                Console.WriteLine($"Exception while trying to set secret {secretName}, {ex}");
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

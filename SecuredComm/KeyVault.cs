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
        private readonly string m_applicationId;
        private readonly string m_applicationSecret;

        #endregion

        public string Url { get; private set; }

        // todo: error + logs
        public KeyVault(string kvUrl, string applicationId, string applicationSecret)
        {
            Url = kvUrl;
            m_applicationId = applicationId;
            m_applicationSecret = applicationSecret;
           
            m_kvClient = new KeyVaultClient(GetAccessTokenAsync, new HttpClient());
        }

        public async Task<SecretBundle> GetSecretAsync(string secretName)
        {
            try
            {
                return await m_kvClient.GetSecretAsync(Url, secretName);
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
                return await m_kvClient.SetSecretAsync(Url, secretName, value);
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

using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

namespace SecuredCommunication
{
    public class KeyVault : IKeyVault
    {
        public string Url;
        public KeyVaultClient client;

        public KeyVault(string kvUrl)
        {
            Url = kvUrl;
            client = new KeyVaultClient(
                                    new KeyVaultClient.AuthenticationCallback(GetAccessTokenAsync),
                                    new HttpClient());
        }

        public string GetUrl()
        {
            return this.Url;
        }

        public Task<SecretBundle> GetSecretAsync(string vault, string secretName) 
        { 
            return client.GetSecretAsync(vault, secretName);;
        } 

        public Task<SecretBundle> SetSecretAsync(string vault, string secretName, string value)
        {
            return this.client.SetSecretAsync(vault, secretName, value);
        }

        public Task<KeyBundle> GetKeyAsync(string vault,
                               string keyName,
                               string keyVersion = null)
        {

            return client.GetKeyAsync(this.Url, keyName, null);

        }

        private static async Task<string> GetAccessTokenAsync(
            string authority,
            string resource,
            string scope)
        {
            //clientID and clientSecret are obtained by registering
            //the application in Azure AD
            var clientId = ConfigurationManager.AppSettings["clientId"];
            var clientSecret = ConfigurationManager.AppSettings["clientSecret"];

            var clientCredential = new ClientCredential(clientId, clientSecret);
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, clientCredential);

            return result.AccessToken;
        }
    }
}

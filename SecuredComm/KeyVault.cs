using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
using System;
using Contracts;

namespace SecuredCommunication
{
    public class KeyVault : IKeyVault
    {
        public string Url;
        private const string publicKeySuffix = "-public";
        private const string privateKeySuffix = "-private";
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

        public Task<SecretBundle> GetSecretAsync(string secretName) 
        { 
            return client.GetSecretAsync(GetUrl(), secretName);;
        } 

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            return client.SetSecretAsync(GetUrl(), secretName, value);
        }

        public Task<KeyBundle> GetKeyAsync(string keyName, string keyVersion = null)
        {

            return client.GetKeyAsync(this.Url, keyName, null);

        }

        public async Task<string> GetPrivateKeyAsync(string identifier)
        {
            var secret = await client.GetSecretAsync(GetUrl(), identifier + privateKeySuffix);
            return secret.Value;
        }

        public async Task<string> GetPublicKeyAsync(string identifier)
        {
            var secret = await client.GetSecretAsync(GetUrl(), identifier + publicKeySuffix); 
            return secret.Value;
        }

        public async Task<bool> StoreKeyPairAsync(string identifier, KeyPair key)
        {
            try
            {
                await client.SetSecretAsync(GetUrl(), identifier + publicKeySuffix, key.PublicKey);
                await client.SetSecretAsync(GetUrl(), identifier + privateKeySuffix, key.PrivateKey);
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                throw;
            }
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

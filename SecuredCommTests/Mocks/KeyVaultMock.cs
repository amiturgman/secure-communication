using Cryptography;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

namespace UnitTests
{
    public class KeyVaultMock : IKeyVault
    {
        private string kvUri;

        public KeyVaultMock(string kvUri)
        {
            this.kvUri = kvUri;
        }

        public string GetUrl()
        {
            return kvUri;
        }

        public Task<SecretBundle> GetSecretAsync(string secretName)
        {
            Console.WriteLine("Starting get secret");
            if (secretName.Equals("sender"))
            {
                return Task.FromResult(new SecretBundle(TestConstants.privateKey));
            }

            var x = new X509Certificate2("../../../testCert.pfx", "abc123ABC", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            //var key = await GetKeyAsync(secretName);
            byte[] certBytes = x.Export(X509ContentType.Pkcs12);
            var secBundle = new SecretBundle(Convert.ToBase64String(certBytes));
            Console.WriteLine("finished get secret");
            return Task.FromResult(secBundle);
        }

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StoreKeyPairAsync(string identifier, string key)
        {
            throw new NotImplementedException();
        }
    }
}

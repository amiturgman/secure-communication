using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
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

        public async Task<SecretBundle> GetSecretAsync(string secretName)
        {
            if (secretName.Equals("sender-public"))
            {
                return new SecretBundle(TestConstants.publicKey);
            }

            if (secretName.Equals("sender-private"))
            {
                return new SecretBundle(TestConstants.privateKey);
            }

            var x = new X509Certificate2("../../../testCert.pfx", "abc123ABC", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            //var key = await GetKeyAsync(secretName);
            byte[] certBytes = x.Export(X509ContentType.Pkcs12);
            var secBundle = new SecretBundle(Convert.ToBase64String(certBytes));
            return secBundle;
        }

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            throw new Exception();
        }

        public Task<bool> StoreKeyPairAsync(string identifier, KeyPair key)
        {
            throw new NotImplementedException();
        }
    }
}

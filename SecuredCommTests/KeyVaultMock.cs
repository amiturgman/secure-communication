using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Azure.KeyVault.Models;
using Org.BouncyCastle.Utilities.Encoders;

namespace UnitTests
{
    public class KeyVaultMock : IKeyVault
    {
        private string kvUri;

        public KeyVaultMock(string kvUri)
        {
            this.kvUri = kvUri;
        }

        public Task<string> GetPrivateKeyAsync(string identifier)
        {
            return Task.FromResult(TestConstants.privateKey);
        }

        public Task<KeyOperationResult> EncryptAsync(string keyIdentifier, string algorithm, byte[] value)
        {
            var encryptedData = new byte[256];
            for (int i = 0; i < encryptedData.Length; i++)
            {
                encryptedData[i] = 0x20;
            }
            return Task.FromResult(new KeyOperationResult(keyIdentifier, encryptedData));
        }

        public Task<KeyOperationResult> DecryptAsync(string keyIdentifier, string algorithm, byte[] value)
        {
            return Task.FromResult(new KeyOperationResult(keyIdentifier, Utils.ToByteArray("Hi")));
        }

        public Task<string> GetPublicKeyAsync(string identifier)
        {
            return Task.FromResult(TestConstants.publicKey);
        }

        public async Task<SecretBundle> GetSecretAsync(string secretName)
        {
            var x = new X509Certificate2("../../../testCert.pfx", "abc123ABC", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            //var key = await GetKeyAsync(secretName);
            byte[] certBytes = x.Export(X509ContentType.Pkcs12);
            var secBundle = new SecretBundle(Convert.ToBase64String(certBytes));
            return secBundle;
        }

        public string GetUrl()
        {
            return kvUri;
        }

        public Task<KeyBundle> GetKeyAsync(string keyName)
        {

            var x = new X509Certificate2("../../../testCert.pfx", "abc123ABC", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var key = keyName.Contains("private") ? x.GetRSAPrivateKey() : x.GetRSAPublicKey();
            using (RSA rsa = key)
            {
                var shouldGetPrivate = keyName.Contains("private");
                var parameters = rsa.ExportParameters(shouldGetPrivate);
                KeyBundle bundle = new KeyBundle
                { 
                    Key = new Microsoft.Azure.KeyVault.WebKey.JsonWebKey
                    {
                        Kty = Microsoft.Azure.KeyVault.WebKey.JsonWebKeyType.Rsa,
                        // Private stuff
                        D = parameters.D,
                        DP = parameters.DP,
                        DQ = parameters.DQ,
                        P = parameters.P,
                        Q = parameters.Q,
                        QI = parameters.InverseQ,
                        // Public stuff
                        N = parameters.Modulus,
                        E = parameters.Exponent, 
                        Kid = "https://mykv.vault.azure.net:443/keys/" + keyName + "/"
                    },
                };
                return Task.FromResult(bundle);
            }
        }

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            throw new Exception();
        }

        public Task<bool> StoreKeyPairAsync(string identifier, KeyPair key)
        {
            throw new NotImplementedException();
        }

        byte[] sig = Utils.ToByteArray<string>("Signature");
        public Task<KeyOperationResult> SignAsync(string keyIdentifier, string algorithm, byte[] digest)
        {
            return Task.FromResult(new KeyOperationResult(keyIdentifier, sig));
        }

        public Task<bool> VerifyAsync(string keyIdentifier, string algorithm, byte[] digest, byte[] signature)
        {
            return Task.FromResult(true);
         }
    }
}

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Azure.KeyVault.Models;

namespace SecuredCommTests
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
            return Task.FromResult(new KeyOperationResult(result: new byte[] {}));
        }

        public Task<KeyOperationResult> DecryptAsync(string keyIdentifier, string algorithm, byte[] value)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPublicKeyAsync(string identifier)
        {
            return Task.FromResult(TestConstants.publicKey);
        }

        public Task<SecretBundle> GetSecretAsync(string secretName)
        {
            throw new Exception();
        }

        public string GetUrl()
        {
            throw new NotImplementedException();
        }

        public Task<SecretBundle> SetSecretAsync(string secretName, string value)
        {
            throw new Exception();
        }

        public Task<bool> StoreKeyPairAsync(string identifier, KeyPair key)
        {
            throw new NotImplementedException();
        }

        Task<KeyBundle> IKeyVault.GetKeyAsync(string keyName, string keyVersion)
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
                    },
                };
                return Task.FromResult(bundle);
            }
        }

        public Task<KeyOperationResult> SignAsync(string keyIdentifier, string algorithm, byte[] digest)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyAsync(string keyIdentifier, string algorithm, byte[] digest, byte[] signature)
        {
            throw new NotImplementedException();
        }
    }
}

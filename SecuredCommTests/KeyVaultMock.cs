using System;
using System.Security.Cryptography;
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

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Azure.KeyVault.Models;
using SecuredCommunication;

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
            try
            {
                var key = GetPublicKeyAsync(keyIdentifier);
                byte[] encryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                 //   RSA.ImportParameters(new RSAParameters() {}(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(value, false);
                }
                return Task.FromResult(new KeyOperationResult(Utils.FromByteArray<string>(encryptedData)));
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }        }

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

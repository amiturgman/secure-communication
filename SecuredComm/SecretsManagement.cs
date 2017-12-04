using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class SecretsManagement : ISecretsManagement
    {
        private List<IKeyVault> m_KeyVaultList;
        private const string publicKeySuffix = "-public";
        private const string privateKeySuffix = "-private";

        public SecretsManagement(IKeyVault keyVault)
        {
            m_KeyVaultList = new List<IKeyVault>();
            AddKeyVault(keyVault);
        }

        public void AddKeyVault(IKeyVault keyVault)
        {
            if (m_KeyVaultList.Exists(kv => keyVault.GetUrl().Equals(kv.GetUrl())))//keyVault.Url.Equals(kv.Url)))
            {
                throw new Exception($"Key Vault with name {keyVault.GetUrl()} already exists");
            }

            m_KeyVaultList.Add(keyVault);
        }

        public async Task<string> Decrypt(string keyVaultUrl, string keyName, string encryptedData)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            var keyVault = LoadKeyVault(keyVaultUrl);

            try
            {
                // var key = await keyVault.client.GetKeyAsync(keyVault.Url, keyName, null);
                var key = await keyVault.GetKeyAsync(keyVault.GetUrl(), keyName, null);

                using (var rsa = new RSACryptoServiceProvider())
                {
                    var p = new RSAParameters() { Modulus = key.Key.N, Exponent = key.Key.E, D=key.Key.D,  DP = key.Key.DP, DQ = key.Key.DQ,  InverseQ = key.Key.QI,  P = key.Key.P,  Q = key.Key.Q };
                    rsa.ImportParameters(p);

                    // Decrypt
                    var encryptedTextNew = Convert.FromBase64String(encryptedData);
                    var decryptedData = rsa.Decrypt(encryptedTextNew, true);

                    var decryptedText = Encoding.Unicode.GetString(decryptedData);

                    return decryptedText;
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception was thrown: " + exc);
                throw;
            }
        }

        public async Task<string> Encrypt(string keyVaultUrl, string keyName, string data)
        {
            //// For encryption use the global KV 
            //// (the one with just public keys).
            var keyVault = LoadKeyVault(keyVaultUrl);

            try
            {
                var key = await keyVault.GetKeyAsync(keyVault.GetUrl(), keyName, null);

                var publicKey = Convert.ToBase64String(key.Key.N);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var p = new RSAParameters() { Modulus = key.Key.N, Exponent = key.Key.E };
                    rsa.ImportParameters(p);
                    var byteData = Encoding.Unicode.GetBytes(data);

                    // Encrypt
                    var encryptedText = rsa.Encrypt(byteData, true);
                    return Convert.ToBase64String(encryptedText);
               }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GetPrivateKey(string keyVaultUrl, string identifier)
        {
            var keyVault = LoadKeyVault(keyVaultUrl);
            var secret = await keyVault.GetSecretAsync(keyVault.GetUrl(), identifier + privateKeySuffix);
            return secret.Value;
        }

        public async Task<string> GetPublicKey(string keyVaultUrl, string identifier)
        {
            var keyVault = LoadKeyVault(keyVaultUrl);
            var secret = await keyVault.GetSecretAsync(keyVault.GetUrl(), identifier + publicKeySuffix); //.client.GetSecretAsync(keyVault.GetUrl(), identifier + publicKeySuffix);
            return secret.Value;
        }

        public async Task<byte[]> Sign(string keyVaultUrl, string keyName, string data)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            var keyVault = LoadKeyVault(keyVaultUrl);
            var digest = calculateDigest(data);

            var key = await keyVault.GetKeyAsync(keyVault.GetUrl(), keyName, null);

            //var signature = await keyVault.SignAsync(key.KeyIdentifier.Identifier, "RS256", digest);
            //return signature.Result;
            return await Task.FromResult(new byte[] { });
        }

        public async Task<bool> Verify(string keyVaultUrl, string keyName, byte[] signature, string data)
        {
            //// For encryption use the global KV 
            //// (the one with just public keys).
            var keyVault = LoadKeyVault(keyVaultUrl);
            var key = await keyVault.GetKeyAsync(keyVault.GetUrl(), keyName, null);

            //var verify = await keyVault.client.VerifyAsync(key.KeyIdentifier.Identifier, "RS256", calculateDigest(data), signature);
            //return verify;
            return await Task.FromResult(true);
        }

        public async Task<bool> StoreKeyPair(string keyVaultUrl, string identifier, KeyPair key)
        {
            var keyVault = LoadKeyVault(keyVaultUrl);
            try {
                await keyVault.SetSecretAsync(keyVault.GetUrl(), identifier + publicKeySuffix, key.PublicKey);
                await keyVault.SetSecretAsync(keyVault.GetUrl(), identifier + privateKeySuffix, key.PrivateKey);
                return true;
            } catch (Exception)
            {
                throw;
            }
        }

        #region Private Methods

        private IKeyVault LoadKeyVault(string keyVaultUrl)
        {
            foreach (var keyVault in m_KeyVaultList.Where(keyVault => keyVaultUrl.Equals(keyVault.GetUrl())))
            {
                return keyVault;
            }

            throw new Exception("Key vault doesn't exist");
        }

        private byte[] calculateDigest(string data)
        {
            var hasher = new SHA256CryptoServiceProvider();
            var byteData = Encoding.Unicode.GetBytes(data);
            var digest = hasher.ComputeHash(byteData);
            return digest;
        }
        #endregion
    }
}

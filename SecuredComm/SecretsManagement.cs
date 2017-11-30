using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
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
        private List<KeyVaultInfo> m_KeyVaultList;
        private const string publicKeySuffix = "-public";
        private const string privateKeySuffix = "-private";

        public SecretsManagement(KeyVaultInfo keyVault)
        {
            m_KeyVaultList = new List<KeyVaultInfo>();
            AddKeyVault(keyVault);
        }

        public void AddKeyVault(KeyVaultInfo keyVault)
        {
            if (m_KeyVaultList.Exists(kv => keyVault.Url.Equals(kv.Url)))
            {
                throw new Exception($"Key Vault with name {keyVault.Url} already exists");
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
                var key = await keyVault.client.GetKeyAsync(keyVault.Url, keyName, null);

                var publicKey = Convert.ToBase64String(key.Key.N);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    var p = new RSAParameters() { Modulus = key.Key.N, Exponent = key.Key.E };
                    rsa.ImportParameters(p);

                    // Decrypt
                    var encryptedTextNew = Convert.FromBase64String(encryptedData);
                    var decryptedData = keyVault.client.DecryptAsync(key.KeyIdentifier.Identifier, JsonWebKeyEncryptionAlgorithm.RSAOAEP, encryptedTextNew).GetAwaiter().GetResult();
                    var decryptedText = Encoding.Unicode.GetString(decryptedData.Result);

                    return decryptedText;
                }
            }
            catch (Exception)
            {
                //TODO: handle exception
                return "";
            }
        }

        public async Task<string> Encrypt(string keyVaultUrl, string keyName, string data)
        {
            // For encryption use the global KV 
            // (the one with just public keys).
            var keyVault = LoadKeyVault(keyVaultUrl);

            try
            {
                var key = await keyVault.client.GetKeyAsync(keyVault.Url, keyName, null);

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
                return "";
            }

        }

        public async Task<string> GetPrivateKey(string keyVaultUrl, string identifier)
        {
            var keyVault = LoadKeyVault(keyVaultUrl);
            var secret = await keyVault.client.GetSecretAsync(keyVault.Url, identifier + privateKeySuffix);
            return secret.Value;
        }

        public async Task<string> GetPublicKey(string keyVaultUrl, string identifier)
        {
            var keyVault = LoadKeyVault(keyVaultUrl);
            var secret = await keyVault.client.GetSecretAsync(keyVault.Url, identifier + publicKeySuffix);
            return secret.Value;
        }

        public async Task<byte[]> Sign(string keyVaultUrl, string keyName, string data)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            var keyVault = LoadKeyVault(keyVaultUrl);
            var digest = calculateDigest(data);

            var key = await keyVault.client.GetKeyAsync(keyVault.Url, keyName, null);

            var signature = await keyVault.client.SignAsync(key.KeyIdentifier.Identifier, "RS256", digest);
            return signature.Result;
        }

        public async Task<bool> Verify(string keyVaultUrl, string keyName, byte[] signature, string data)
        {
            // For encryption use the global KV 
            // (the one with just public keys).
            var keyVault = LoadKeyVault(keyVaultUrl);
            var key = await keyVault.client.GetKeyAsync(keyVault.Url, keyName, null);

            var verify = await keyVault.client.VerifyAsync(key.KeyIdentifier.Identifier, "RS256", calculateDigest(data), signature);
            return verify;
        }

        public async Task<bool> StoreKeyPair(string keyVaultUrl, string identifier, KeyPair key)
        {
            var keyVault = LoadKeyVault(keyVaultUrl);
            try {
                await keyVault.client.SetSecretAsync(keyVault.Url, identifier + publicKeySuffix, key.PublicKey);
                await keyVault.client.SetSecretAsync(keyVault.Url, identifier + privateKeySuffix, key.PrivateKey);
                return true;
            } catch (Exception ex)
            {
                //TODO: handle exception
                return false;
            }
        }
        #region Private Methods
        private KeyVaultInfo LoadKeyVault(string keyVaultUrl)
        {
            foreach (KeyVaultInfo keyVault in m_KeyVaultList.Where(keyVault => keyVaultUrl.Equals(keyVault.Url)))
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

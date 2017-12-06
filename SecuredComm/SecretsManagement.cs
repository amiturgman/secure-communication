using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Contracts;

namespace SecuredCommunication
{
    public class SecretsManagement : ISecretsManagement
    {
        private IKeyVault m_privateKeyVault;
        private IKeyVault m_publicKeyVault;


        private string m_decryptionKeyName;
        private string m_encryptionKeyName;
        private string m_signKeyName;
        private string m_verifyKeyName;


        public SecretsManagement(string encryptionKeyName, string decryptionKeyName, string signKeyName, string verifyKeyName, IKeyVault privateKv, IKeyVault publicKv)
        {
            m_decryptionKeyName = decryptionKeyName;
            m_encryptionKeyName = encryptionKeyName;
            m_signKeyName = signKeyName;
            m_verifyKeyName = verifyKeyName;

            m_privateKeyVault = privateKv;
            m_publicKeyVault = publicKv;
        }

        public async Task<string> Decrypt(string encryptedData)
        {
            try
            {
                // var key = await keyVault.client.GetKeyAsync(keyVault.Url, keyName, null);
                var key = await m_privateKeyVault.GetKeyAsync(m_decryptionKeyName, null);

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

        public async Task<string> Encrypt(string data)
        {
            try
            {
                var key = await m_publicKeyVault.GetKeyAsync(m_encryptionKeyName, null);

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

        public async Task<byte[]> Sign(string data)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            //var digest = calculateDigest(data);

            //var key = await m_privateKeyVault.GetKeyAsync(m_signKeyName, null);

            //var signature = await keyVault.SignAsync(key.KeyIdentifier.Identifier, "RS256", digest);
            //return signature.Result;
            return await Task.FromResult(new byte[] { });
        }

        public async Task<bool> Verify(byte[] signature, string data)
        {
            //// For encryption use the global KV 
            //// (the one with just public keys).
            //var key = await m_publicKeyVault.GetKeyAsync(m_verifyKeyName, null);

            //var verify = await keyVault.client.VerifyAsync(key.KeyIdentifier.Identifier, "RS256", calculateDigest(data), signature);
            //return verify;
            return await Task.FromResult(false);
        }

        #region Private Methods

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

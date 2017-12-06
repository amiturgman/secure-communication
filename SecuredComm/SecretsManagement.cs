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

        public async Task<string> Decrypt(byte[] encryptedData)
        {
            try
            {
                var key = await m_privateKeyVault.GetKeyAsync(m_decryptionKeyName);
                var result = await m_privateKeyVault.DecryptAsync(key.KeyIdentifier.Identifier, "RSA1_5", encryptedData);
                return Encoding.Unicode.GetString(result.Result);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception was thrown: " + exc);
                throw;
            }
        }

        public async Task<byte[]> Encrypt(string data)
        {
            try
            {
                var key = await m_publicKeyVault.GetKeyAsync(m_encryptionKeyName);
                var result = await m_publicKeyVault.EncryptAsync(key.KeyIdentifier.Identifier, "RSA1_5", Encoding.Unicode.GetBytes(data));
                return result.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public async Task<byte[]> Sign(string data)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            //var digest = calculateDigest(data);

            //var key = await m_privateKeyVault.GetKeyAsync(m_signKeyName);

            //var signature = await keyVault.SignAsync(key.KeyIdentifier.Identifier, "RS256", digest);
            //return signature.Result;
            return await Task.FromResult(new byte[] { });
        }

        public async Task<bool> Verify(byte[] signature, string data)
        {
            //// For encryption use the global KV 
            //// (the one with just public keys).
            //var key = await m_publicKeyVault.GetKeyAsync(m_verifyKeyName);

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

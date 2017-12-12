using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Org.BouncyCastle.Utilities.Encoders;

namespace SecuredCommunication
{
    public class SecretsManagement : ISecretsManagement
    {
        
        #region private memebers

        private IKeyVault m_privateKeyVault;
        private IKeyVault m_publicKeyVault;

        private string m_decryptionKeyName;
        private string m_encryptionKeyName;
        private string m_signKeyName;
        private string m_verifyKeyName;

        #endregion


        public SecretsManagement(string encryptionKeyName, string decryptionKeyName, string signKeyName, string verifyKeyName, IKeyVault privateKv, IKeyVault publicKv)
        {
            m_decryptionKeyName = decryptionKeyName;
            m_encryptionKeyName = encryptionKeyName;
            m_signKeyName = signKeyName;
            m_verifyKeyName = verifyKeyName;

            m_privateKeyVault = privateKv;
            m_publicKeyVault = publicKv;
        }

        public async Task<byte[]> Decrypt(byte[] encryptedData)
        {
            try
            {
                var secret = await m_publicKeyVault.GetSecretAsync("PfxFile");
                var x509 = new X509Certificate2(Base64.Decode(secret.Value));
                return DecryptDataOaepSha1(x509, encryptedData);
                //var key = await m_privateKeyVault.GetKeyAsync(m_decryptionKeyName);
                //var result = await m_privateKeyVault.DecryptAsync(key.KeyIdentifier.Identifier, "RSA1_5", encryptedData);
                //return result.Result;
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception was thrown: " + exc);
                throw;
            }
        }

        public static byte[] DecryptDataOaepSha1(X509Certificate2 cert, byte[] data)
        {
            // GetRSAPrivateKey returns an object with an independent lifetime, so it should be
            // handled via a using statement.
            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                return rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public async Task<byte[]> Encrypt(byte[] data)
        {
            try
            {
                var secret = await m_publicKeyVault.GetSecretAsync("PfxFile");
                var x509 = new X509Certificate2(Base64.Decode(secret.Value));
                var encrypted = EncryptDataOaepSha1(x509, data);
                return encrypted;
                //var key = await m_publicKeyVault.GetKeyAsync(m_encryptionKeyName);
                //var result = await m_publicKeyVault.EncryptAsync(key.KeyIdentifier.Identifier, "RSA1_5", data);
                //return result.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public static byte[] EncryptDataOaepSha1(X509Certificate2 cert, byte[] data)
        {
            // GetRSAPublicKey returns an object with an independent lifetime, so it should be
            // handled via a using statement.
            using (RSA rsa = cert.GetRSAPublicKey())
            {
                // OAEP allows for multiple hashing algorithms, what was formermly just "OAEP" is
                // now OAEP-SHA1.
                return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);
            }
        }

        public Task<byte[]> SignAsync(byte[] data)
        {
            //For encryption use the private KV
            //(the one associated with the current service).

            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

            RSAParameters Key = RSAalg.ExportParameters(true);

            try
            {
                RSAalg.ImportParameters(Key);

                return Task.FromResult(RSAalg.SignData(data, new SHA1CryptoServiceProvider()));
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                throw;
            }

            /* var digest = CalculateDigest(data);
            var key = await m_privateKeyVault.GetKeyAsync(m_signKeyName);
            var signature = await m_privateKeyVault.SignAsync(key.KeyIdentifier.Identifier, "RS256", digest);
            return signature.Result; */
        }

        public Task<bool> VerifyAsync(byte[] data, byte[] signature)
        {
            //// For encryption use the global KV 
            //// (the one with just public keys).
            /* var key = await m_publicKeyVault.GetKeyAsync(m_verifyKeyName);

            var verify = await m_publicKeyVault.VerifyAsync(key.KeyIdentifier.Identifier, "RS256", CalculateDigest(data), signature);
            return verify; */
            try
            {
                // Create a new instance of RSACryptoServiceProvider using the 
                // key from RSAParameters.
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

                RSAParameters Key = RSAalg.ExportParameters(false);

                RSAalg.ImportParameters(Key);

                return Task.FromResult(RSAalg.VerifyData(data, new SHA1CryptoServiceProvider(), signature));

            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        #region Private Methods

        private byte[] CalculateDigest(byte[] data)
        {
            var hasher = new SHA256CryptoServiceProvider();
            var digest = hasher.ComputeHash(data);
            return digest;
        }
        #endregion
    }
}

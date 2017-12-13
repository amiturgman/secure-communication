using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Org.BouncyCastle.Utilities.Encoders;

namespace SecuredCommunication
{
    public class KeyVaultSecretManager : IEncryptionManager
    {

        #region private memebers

        private IKeyVault m_privateKeyVault;
        private IKeyVault m_publicKeyVault;

        private string m_decryptionKeyName;
        private string m_encryptionKeyName;
        private string m_signKeyName;
        private string m_verifyKeyName;

        #endregion


        public KeyVaultSecretManager(string encryptionKeyName, string decryptionKeyName, string signKeyName, string verifyKeyName, IKeyVault privateKv, IKeyVault publicKv)
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
                var secret = await m_privateKeyVault.GetSecretAsync(m_decryptionKeyName);
                var x509 = new X509Certificate2(Base64.Decode(secret.Value));
                return DecryptDataOaepSha1(x509, encryptedData);
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
                var secret = await m_publicKeyVault.GetSecretAsync(m_encryptionKeyName);
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

        public static byte[] SignData(X509Certificate2 cert, byte[] data)
        {
            using (RSA rsa = cert.GetRSAPrivateKey())
            {
                return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        public static bool VerifyData(X509Certificate2 cert, byte[] data, byte[] signature)
        {
            using (RSA rsa = cert.GetRSAPublicKey())
            {
                return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        public async Task<byte[]> SignAsync(byte[] data)
        {
            //For encryption use the private KV
            //(the one associated with the current service).

            var secret = await m_publicKeyVault.GetSecretAsync(m_signKeyName);
            var x509 = new X509Certificate2(Base64.Decode(secret.Value));
            return SignData(x509, data);
        }

        public async Task<bool> VerifyAsync(byte[] data, byte[] signature)
        {
            //// For encryption use the global KV 
            //// (the one with just public keys).
            var secret = await m_publicKeyVault.GetSecretAsync(m_verifyKeyName);
            var x509 = new X509Certificate2(Base64.Decode(secret.Value));
            return VerifyData(x509, data, signature);
        }
    }
}
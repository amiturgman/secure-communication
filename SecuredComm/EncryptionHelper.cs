using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;

namespace SecuredCommunication
{
    /// <summary>
    /// Manages the crypto operations which can be applied on a message using the loaded certificates.
    /// </summary>
    public class EncryptionHelper : IEncryptionManager
    {
        #region private members

        private readonly X509Certificate2 m_encryptionCert;
        private readonly X509Certificate2 m_decryptionCert;
        private readonly X509Certificate2 m_signCert;
        private readonly X509Certificate2 m_verifyCert;

        #endregion

        public EncryptionHelper(X509Certificate2 encryptionCert, X509Certificate2 decryptionCert, X509Certificate2 signCert, X509Certificate2 verifyCert)
        {
            m_signCert = signCert;
            m_verifyCert = verifyCert;
            m_encryptionCert = encryptionCert;
            m_decryptionCert = decryptionCert;
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            try
            {
                // GetRSAPrivateKey returns an object with an independent lifetime, so it should be
                // handled via a using statement.
                using (RSA rsa = m_decryptionCert.GetRSAPrivateKey())
                {
                    return rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA1);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception was thrown: " + exc);
                throw;
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            try
            {
                // GetRSAPublicKey returns an object with an independent lifetime, so it should be
                // handled via a using statement.
                using (RSA rsa = m_encryptionCert.GetRSAPublicKey())
                {
                    // OAEP allows for multiple hashing algorithms, what was formermly just "OAEP" is
                    // now OAEP-SHA1.
                    return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
       
        public byte[] Sign(byte[] data)
        {
            using (RSA rsa = m_signCert.GetRSAPrivateKey())
            {
                return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            using (RSA rsa = m_verifyCert.GetRSAPublicKey())
            {
                return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
    }
}
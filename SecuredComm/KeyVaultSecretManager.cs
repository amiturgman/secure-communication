using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Org.BouncyCastle.Utilities.Encoders;

namespace SecuredCommunication
{
    /// <summary>
    /// An implementation of <see cref="IEncryptionManager"/>, in this implemetation the certificates
    /// are loaded from two given key vaults
    /// </summary>
    public class KeyVaultSecretManager : IEncryptionManager
    {
        #region private memebers

        private IKeyVault m_privateKeyVault;
        private IKeyVault m_publicKeyVault;

        private string m_decryptionKeyName;
        private string m_encryptionKeyName;
        private string m_signKeyName;
        private string m_verifyKeyName;

        // todo : get rid
        private X509Certificate2 m_encryptionCert;
        private X509Certificate2 m_decryptionCert;
        private X509Certificate2 m_signCert;
        private X509Certificate2 m_verifyCert;
        private EncryptionHelper m_encryptionHelper;
        private bool m_isInit;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SecuredCommunication.KeyVaultSecretManager"/> class.
        /// </summary>
        /// <param name="encryptionKeyName">Encryption key name.</param>
        /// <param name="decryptionKeyName">Decryption key name.</param>
        /// <param name="signKeyName">Sign key name.</param>
        /// <param name="verifyKeyName">Verify key name.</param>
        /// <param name="privateKv">A KV with private keys. Will be used for decryption and signing</param>
        /// <param name="publicKv">A KV just with public keys. Will be used for encryption and verifying</param>
        public KeyVaultSecretManager(string encryptionKeyName, string decryptionKeyName, string signKeyName, string verifyKeyName, IKeyVault privateKv, IKeyVault publicKv)
        {
            // marked as false as we still need to initialize the EncryptionHelper later
            m_isInit = false;

            m_decryptionKeyName = decryptionKeyName;
            m_encryptionKeyName = encryptionKeyName;
            m_signKeyName = signKeyName;
            m_verifyKeyName = verifyKeyName;

            m_privateKeyVault = privateKv;
            m_publicKeyVault = publicKv;
        }

        /// <summary>
        /// Initialize the <see cref="EncryptionHelper"/> object with all the certificates taken from the keyvaults
        /// </summary>
        private async Task Initialize() {

            var encryptSecretTask = m_publicKeyVault.GetSecretAsync(m_encryptionKeyName);
            var decryptSecretTask = m_privateKeyVault.GetSecretAsync(m_decryptionKeyName);
            var signSecretTask = m_publicKeyVault.GetSecretAsync(m_signKeyName);
            var verifySecretTask = m_publicKeyVault.GetSecretAsync(m_verifyKeyName);

            // wait on all of the tasks concurrently
            var tasks = new Task[] { encryptSecretTask, decryptSecretTask, signSecretTask, verifySecretTask };
            await Task.WhenAll(tasks);

            // when using 'Result' we know that the task is actually done already
            m_encryptionCert = SecretToCertificate(encryptSecretTask.Result.Value);
            m_decryptionCert = SecretToCertificate(decryptSecretTask.Result.Value);
            m_signCert = SecretToCertificate(signSecretTask.Result.Value);
            m_verifyCert = SecretToCertificate(verifySecretTask.Result.Value);

            // Now, we have an 'EncryptionHelper', which can help us encrypt, decrypt, sign and verify using
            // the prefetched certificates
            m_encryptionHelper = 
                new EncryptionHelper(m_encryptionCert, m_decryptionCert, m_signCert, m_verifyCert);

            m_isInit = true;
        }

        /// <summary>
        /// Takes a Base64 representation of a certificate and creates a new certificate
        /// object
        /// </summary>
        /// <returns>The certificate object</returns>
        /// <param name="secret">Base64 string representation of a certificate</param>
        private X509Certificate2 SecretToCertificate(string secret) {
            return new X509Certificate2(Base64.Decode(secret));
        }

        /// <summary>
        /// Throw exception if not initialized
        /// </summary>
        private void VerifyInitialized(){
            if (!m_isInit) {
                throw new Exception("Initialize first...");
            }
        }


        public byte[] Decrypt(byte[] encryptedData)
        {
            VerifyInitialized();

            try
            {
                return m_encryptionHelper.Decrypt(encryptedData);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Exception was thrown: " + exc);
                throw;
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            VerifyInitialized();

            try
            {
                return m_encryptionHelper.Encrypt(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public byte[] Sign(byte[] data)
        {
            VerifyInitialized();

            try
            {
                return m_encryptionHelper.Sign(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            VerifyInitialized();

            try
            {
                return m_encryptionHelper.Verify(data, signature);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
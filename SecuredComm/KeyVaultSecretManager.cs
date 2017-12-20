using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Contracts;
using Org.BouncyCastle.Utilities.Encoders;
using SecuredComm;

namespace SecuredCommunication
{
    /// <summary>
    /// An implementation of <see cref="IEncryptionManager"/>, in this implementation the certificates
    /// are loaded from two given key vaults
    /// </summary>
    public class KeyVaultSecretManager : IEncryptionManager
    {
        #region private members

        private readonly IKeyVault m_privateKeyVault;
        private readonly IKeyVault m_publicKeyVault;

        private readonly string m_decryptionKeyName;
        private readonly string m_encryptionKeyName;
        private readonly string m_signKeyName;
        private readonly string m_verifyKeyName;

        private EncryptionHelper m_encryptionHelper;
        private bool m_isInit;

        #endregion

        #region Private Methods

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
        public async Task Initialize() {

            // todo: handle partiall assignment of values
            var encryptSecretTask = m_publicKeyVault.GetSecretAsync(m_encryptionKeyName);
            var decryptSecretTask = m_privateKeyVault.GetSecretAsync(m_decryptionKeyName);
            var signSecretTask = m_publicKeyVault.GetSecretAsync(m_signKeyName);
            var verifySecretTask = m_publicKeyVault.GetSecretAsync(m_verifyKeyName);

            // wait on all of the tasks concurrently
            var tasks = new Task[] { encryptSecretTask, decryptSecretTask, signSecretTask, verifySecretTask };
            await Task.WhenAll(tasks);

            // when using 'Result' we know that the task is actually done already
            var encryptionCert = SecretToCertificate(encryptSecretTask.Result.Value);
            var decryptionCert = SecretToCertificate(decryptSecretTask.Result.Value);
            var signCert = SecretToCertificate(signSecretTask.Result.Value);
            var verifyCert = SecretToCertificate(verifySecretTask.Result.Value);

            // Now, we have an 'EncryptionHelper', which can help us encrypt, decrypt, sign and verify using
            // the pre-fetched certificates
            m_encryptionHelper = new EncryptionHelper(encryptionCert, decryptionCert, signCert, verifyCert);

            m_isInit = true;
        }


        public byte[] Decrypt(byte[] encryptedData)
        {
            VerifyInitialized();

            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            // Call Decrypt
            try
            {
                return m_encryptionHelper.Decrypt(encryptedData);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            VerifyInitialized();

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            try
            {
                return m_encryptionHelper.Encrypt(data);
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public byte[] Sign(byte[] data)
        {
            VerifyInitialized();

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return m_encryptionHelper.Sign(data);
        }

        public bool Verify(byte[] data, byte[] signature)
        {
            VerifyInitialized();

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            return m_encryptionHelper.Verify(data, signature);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Takes a Base64 representation of a certificate and creates a new certificate
        /// object
        /// </summary>
        /// <returns>The certificate object</returns>
        /// <param name="secret">Base64 string representation of a certificate</param>
        private static X509Certificate2 SecretToCertificate(string secret)
        {
            if (string.IsNullOrEmpty(secret))
            {
                throw new ArgumentException("secret must be supplied");
            }

            return new X509Certificate2(Base64.Decode(secret));
        }

        /// <summary>
        /// Throw exception if not initialized
        /// </summary>
        private void VerifyInitialized()
        {
            if (!m_isInit)
            {
                throw new SecureCommunicationException("Initialize method needs to be called before accessing class methods");
            }
        }

        #endregion
    }
}
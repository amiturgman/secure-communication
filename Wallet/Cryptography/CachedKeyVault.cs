using System;
using System.Threading.Tasks;
using Wallet.Communication;
using StackExchange.Redis;
using Microsoft.Azure.KeyVault.Models;

namespace Wallet.Cryptography
{
    /// <summary>
    /// KV client with redis cache layer. 
    /// </summary>
    public class CachedKeyVault : ISecretsStore
    {
        #region private members

        private bool m_isInitialized;
        private IDatabase m_db;
        private ConnectionMultiplexer m_redis;
        private readonly string m_connectionString;
        private readonly ICryptoActions m_cryptoActions;
        private readonly ISecretsStore m_keyVault;

        #endregion
        
        public CachedKeyVault(string connectionString, ISecretsStore keyVault, ICryptoActions cryptoActions)
        {
            m_isInitialized = false;
            m_connectionString = connectionString;

            m_keyVault = keyVault ?? throw new ArgumentNullException(nameof(keyVault)); ;
            m_cryptoActions = cryptoActions ?? throw new ArgumentNullException(nameof(cryptoActions));
        }

        public void Initialize()
        {
            if (m_isInitialized)
            {
                throw new SecureCommunicationException("Object was already initialized");
            }

            ConfigurationOptions options = ConfigurationOptions.Parse(m_connectionString);
            m_redis = ConnectionMultiplexer.Connect(options);
            m_db = m_redis.GetDatabase();

            m_isInitialized = true;
        }

        /// <summary>
        /// Stores the secret in the SQL data base.
        /// </summary>
        /// <param name="identifier">The secret id</param>
        /// <param name="privateKey">The secret private key</param>
        public async Task SetSecretAsync(string identifier, string privateKey)
        {
            ThrowIfNotInitialized();

            // The encryptedSecret will be saved ENCRYPTED.
            var encryptedSecret = Utils.FromByteArray<string>(m_cryptoActions.Encrypt(Utils.ToByteArray(privateKey)));
            
            // stored UNEncrypted in keyvault, as keyvault is already safe
            // If a previous encryptedSecret exists, it will be overwritten
            var kvTask = m_keyVault.SetSecretAsync(identifier, privateKey);

            // But ENCRYPTED in redis
            // If a previous encryptedSecret exists, it will be overwritten
            var redisTask = m_db.StringSetAsync(identifier, encryptedSecret);

            await Task.WhenAll(new Task[] { kvTask, redisTask });
        }

        /// <summary>
        /// Gets the secret from the SQL database
        /// </summary>
        /// <param name="identifier">The secret identifier</param>
        /// <returns>The secret from the data base</returns>
        public async Task<string> GetSecretAsync(string identifier)
        {
            ThrowIfNotInitialized();

            var rawValue = await m_db.StringGetAsync(identifier);

            // key not present in redis
            if (!rawValue.IsNullOrEmpty)
            {
                return Utils.FromByteArray<string>(m_cryptoActions.Decrypt(Utils.ToByteArray(rawValue)));
            }

            // Get from KV (returns in unencrypted format)
            var secret = "";
            try
            {
                secret = await m_keyVault.GetSecretAsync(identifier);
            }
            catch (KeyVaultErrorException exc)
            {
                throw new SecureCommunicationException($"key: '{identifier}' was not found in KV", exc);
            }

            // Store in Redis (in Encrypted way)
            await m_db.StringSetAsync(
                identifier, 
                m_cryptoActions.Encrypt(Utils.ToByteArray(secret)));

            return secret;
        }

        #region privateMethods

        private void ThrowIfNotInitialized()
        {
            if (!m_isInitialized)
            {
                throw new SecureCommunicationException("Object was not initialized");
            }
        }
        #endregion
    }
}

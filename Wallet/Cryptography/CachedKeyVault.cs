using System;
using System.Threading.Tasks;
using Wallet.Communication;
using StackExchange.Redis;

namespace Wallet.Cryptography
{
    /// <summary>
    /// RedisConnector allows for secrets to be stored and retrieved from a Redis server. If a CryptoActions instance is provided then the secrets will be stored encrypted.
    /// </summary>
    public class CachedKeyVault : ISecretsStore
    {
        #region private members

        private bool m_isInitialized;
        private IDatabase m_db;
        private ConnectionMultiplexer m_redis;
        private string m_connectionString;
        private ICryptoActions m_cryptoActions;
        private ISecretsStore m_keyVault;

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

            // The value will be saved ENCRYPTED.
            var value = Utils.FromByteArray<string>(m_cryptoActions.Encrypt(Utils.ToByteArray(privateKey)));
            
            // stored UNEncrypted in keyvault, as keyvault is already safe
            var kvTask = m_keyVault.SetSecretAsync(identifier, privateKey);

            // But ENCRYPTED in redis
            var redisTask = m_db.StringSetAsync(identifier, value);

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
            if (rawValue.IsNullOrEmpty)
            {
                // Get from KV (returns in unencrypted format)
                var unEncryptedSecret = await m_keyVault.GetSecretAsync(identifier);

                // Store in Redis (in Encrypted way)
                await m_db.StringSetAsync(
                    identifier, 
                    m_cryptoActions.Encrypt(Utils.ToByteArray(unEncryptedSecret)));

                return unEncryptedSecret;
            }

            return Utils.FromByteArray<string>(m_cryptoActions.Decrypt(Utils.ToByteArray(rawValue)));
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

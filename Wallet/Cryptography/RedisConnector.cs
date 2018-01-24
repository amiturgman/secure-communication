using System;
using System.Threading.Tasks;
using Wallet.Communication;
using StackExchange.Redis;

namespace Wallet.Cryptography
{
    /// <summary>
    /// RedisConnector allows for secrets to be stored and retrieved from a Redis server. If a CryptoActions instance is provided then the secrets will be stored encrypted.
    /// </summary>
    public class RedisConnector : ISecretsStore
    {
        #region public properties

        public bool IsInEncryptMode { get; }
        
        #endregion
        
        #region private members

        private bool m_isInitialized;
        private IDatabase m_db;
        private ConnectionMultiplexer m_redis;
        private string m_connectionString;
        private ICryptoActions m_cryptoActions;

        #endregion

        public RedisConnector(string connectionString)
        {
            m_isInitialized = false;
            m_connectionString = connectionString;
        }

        public RedisConnector(string connectionString, ICryptoActions cryptoActions) : this(connectionString)
        {
            m_cryptoActions = cryptoActions ?? throw new ArgumentNullException(nameof(cryptoActions));

            IsInEncryptMode = true;
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

            // if the RedisConnector was supplied with a CryptoActions implementation, then the value will be saved ENCRYPTED, otherwise UNENCRYPTED.
            var value = IsInEncryptMode ? Utils.FromByteArray<string>(m_cryptoActions.Encrypt(Utils.ToByteArray(privateKey))) : privateKey;
            await m_db.StringSetAsync(identifier, value);
        }

        /// <summary>
        /// Gets the secret from the SQL database
        /// </summary>
        /// <param name="identifier">The secret identifier</param>
        /// <returns>The secret from the data base</returns>
        public async Task<string> GetSecretAsync(string identifier)
        {
            ThrowIfNotInitialized();

            var rawValue = (await m_db.StringGetAsync(identifier)).ToString();
            return IsInEncryptMode ? Utils.FromByteArray<string>(m_cryptoActions.Decrypt(Utils.ToByteArray(rawValue))) : rawValue;
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

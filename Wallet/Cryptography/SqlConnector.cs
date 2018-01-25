using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Wallet.Communication;

namespace Wallet.Cryptography
{
    public class SqlConnector : ISecretsStore
    {
        private SqlConnectionStringBuilder m_sqlConnectionStringBuilder;
        private bool m_isInitialized;

        // SQL queries
        private const string CreateAccountsTableQuery = @"
                            If not exists (select name from sysobjects where name = 'accounts')
                            CREATE TABLE accounts
                            (
                               Id  nchar(30) not null 
                                  PRIMARY KEY,
                               PrivateKey  nvarchar(128)     not null
                            );";

        private const string CreateGetPrivateKeyStoreProcedure = @"
                                IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.Get_PrivateKey'))
                                    exec('CREATE PROC Get_PrivateKey @ID nchar(30)   
	                                AS
	                                SELECT * FROM accounts Where Id=@ID')";

        private const string CreateSetPrivateKeyStoreProcedure = @"
                                IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.Set_PrivateKey'))
                                    exec('CREATE PROC Set_PrivateKey @ID nchar(30), @PrivateKey nvarchar(128)   
                                    AS
                                INSERT INTO accounts (Id, PrivateKey) VALUES (@ID, @PrivateKey)')";


        private const string GetPrivateKeyByIdQueryTemplate = @"Exec Get_PrivateKey '{0}';";
        private const string InsertIntoAccountsTableQueryTemplate = @"Exec Set_PrivateKey '{0}','{1}';";

        public SqlConnector(string userId, string password, string initialCatalog, string dataSource)
        {
            m_isInitialized = false;

            m_sqlConnectionStringBuilder = new SqlConnectionStringBuilder
            {
                UserID = userId,
                Password = password,
                InitialCatalog = initialCatalog,
                DataSource = dataSource,
                IntegratedSecurity = false,
                Encrypt = true,
                TrustServerCertificate = true,
                ConnectTimeout = 60,
                ApplicationIntent = ApplicationIntent.ReadWrite,
                MultiSubnetFailover = false
            };
        }

        public async Task Initialize()
        {
            // Create accounts table and store procedures
            await ExecuteNonQueryAsync(CreateAccountsTableQuery);
            await ExecuteNonQueryAsync(CreateGetPrivateKeyStoreProcedure);
            await ExecuteNonQueryAsync(CreateSetPrivateKeyStoreProcedure);
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
            await ExecuteNonQueryAsync(string.Format(InsertIntoAccountsTableQueryTemplate, identifier, privateKey));
        }

        /// <summary>
        /// Gets the secret from the SQL database
        /// </summary>
        /// <param name="identifier">The secret identifier</param>
        /// <returns>The secret from the data base</returns>
        public async Task<string> GetSecretAsync(string identifier)
        {
            ThrowIfNotInitialized();

            using (var connection = new SqlConnection(m_sqlConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(string.Format(GetPrivateKeyByIdQueryTemplate, identifier), connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            var result = reader.GetString(reader.GetOrdinal("PrivateKey"));
                            return result;
                        }
                    }
                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        #region privateMethods

        private async Task ExecuteNonQueryAsync(string query)
        {
            using (var connection = new SqlConnection(m_sqlConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

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

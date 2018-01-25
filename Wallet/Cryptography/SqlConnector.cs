﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Org.BouncyCastle.Security;
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


        private const string GetPrivateKeyByIdSPName = "Get_PrivateKey";
        private const string InsertIntoAccountsTableSPName = "Set_PrivateKey";

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
            checkForSQLInjection(identifier);
            checkForSQLInjection(privateKey);

            identifier = identifier.Replace("'", "''");
            privateKey = privateKey.Replace("'", "''");

            using (var connection = new SqlConnection(m_sqlConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(InsertIntoAccountsTableSPName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID", identifier);
                        command.Parameters.AddWithValue("@PrivateKey", privateKey);

                        await command.ExecuteReaderAsync();
                    }
                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the secret from the SQL database
        /// </summary>
        /// <param name="identifier">The secret identifier</param>
        /// <returns>The secret from the data base</returns>
        public async Task<string> GetSecretAsync(string identifier)
        {
            ThrowIfNotInitialized();
            checkForSQLInjection(identifier);

            identifier = identifier.Replace("'", "''");

            using (var connection = new SqlConnection(m_sqlConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(GetPrivateKeyByIdSPName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@ID", SqlDbType.NVarChar).Value = identifier;

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            var result = reader.GetString(reader.GetOrdinal("PrivateKey"));
                            return result.Replace("''", "'");
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

        private static void checkForSQLInjection(string input)
        {
            string[] sqlCheckList =
            {
                "--", ";--", ";", "/*", "*/", "@@", "@", "char",
                "nchar", "varchar", "nvarchar", "alter", "begin", "cast",
                "create", "cursor", "declare", "delete", "drop", "end", "exec",
                "execute", "fetch", "insert", "kill", "select", "sys",
                "sysobjects", "syscolumns", "table", "update"
            };

            string checkString = input.Replace("'", "''");

            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
            {
                if (checkString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new InvalidParameterException("Parameters suspected with SQL injection");
                }
            }
        }
    }

    #endregion
}

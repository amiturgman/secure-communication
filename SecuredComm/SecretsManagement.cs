using System;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class SecretsManagement : ISecretsManagement
    {
        public class KvInfo {
            public KvInfo(string kvName, string appId, string principalId) {
                this.kvName = kvName;
                this.appId = appId;
                this.servicePrincipalId = principalId;
            }
            public string kvName;
            public string appId;
            public string servicePrincipalId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SecuredCommunication.SecretsManagement"/> class.
        /// </summary>
        /// <param name="privateKv">Private kv, holds the private-public pairs for both Encryption and Signing</param>
        /// <param name="globalKv">Global kv, holds all the system PUBLIC keys. for Verification and Encryption</param>
        public SecretsManagement(KvInfo privateKv, KvInfo globalKv)
        {
        }

        public async Task<string> Decrypt(string keyName, string encryptedData)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Encrypt(string keyName, string data)
        {
            // For encryption use the global KV 
            // (the one with just public keys).
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Sign(string keyName, string data)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            return await Task.FromResult("Not implemented");
        }

        public async Task<bool> Verify(string keyName, string signature)
        {
            // For encryption use the global KV 
            // (the one with just public keys).
            return await Task.FromResult(false);
        }
    }
}

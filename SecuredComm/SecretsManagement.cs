using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class SecretsManagement : ISecretsManagement
    {
        private List<KeyVaultInfo> _keyVaultList = new List<KeyVaultInfo>();

        public SecretsManagement(KeyVaultInfo keyVault)
        {
            AddKeyVault(keyVault);
        }

        public void AddKeyVault(KeyVaultInfo keyVault)
        {
            if (_keyVaultList.Exists(kv => keyVault.KvName.Equals(kv.KvName)))
            {
                throw new Exception($"Key Vault with name {keyVault.KvName} already exists");
            }

            _keyVaultList.Add(keyVault);
        }

        public async Task<string> Decrypt(string keyVaultName, string keyName, string encryptedData)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Encrypt(string keyVaultName, string keyName, string data)
        {
            // For encryption use the global KV 
            // (the one with just public keys).
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> GetPrivateKey(string keyVaultName, string identifier)
        {
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> GetPublicKey(string keyVaultName, string identifier)
        {
            return await Task.FromResult("Not implemented");
        }

        public async Task<string> Sign(string keyVaultName, string keyName, string data)
        {
            // For encryption use the private KV 
            // (the one associated with the current service).
            return await Task.FromResult("Not implemented");
        }

        public async Task<bool> StoreKeyPair(string keyVaultName, string identifier, KeyPair key)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Verify(string keyVaultName, string keyName, string signature)
        {
            // For encryption use the global KV 
            // (the one with just public keys).
            return await Task.FromResult(false);
        }

        private KeyVaultInfo LoadKeyVault(string KeyVaultName)
        {
            foreach (KeyVaultInfo keyVault in _keyVaultList.Where(keyVault => KeyVaultName.Equals(keyVault.KvName)))
            {
                return keyVault;
            }

            throw new Exception("Key vault doesn't exist");
        }
    }
}

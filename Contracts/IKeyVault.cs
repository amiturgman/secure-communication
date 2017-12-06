﻿using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

namespace Contracts
{
    public interface IKeyVault
    {
        /// <summary>
        /// Gets the uri of the key vault which this object is wrapping
        /// </summary>
        /// <returns>The underlying keyvault's uri</returns>
        string GetUrl();

        /// <summary>
        /// Gets the specified secret
        /// </summary>
        /// <returns>The secret</returns>
        /// <param name="secretName">Secret identifier</param>
        Task<SecretBundle> GetSecretAsync(string secretName);

        /// <summary>
        /// Gets the specified key
        /// </summary>
        /// <returns>The key.</returns>
        /// <param name="keyName">Key identifier.</param>
        /// <param name="keyVersion">Key version.</param>
        Task<KeyBundle> GetKeyAsync(string keyName,
            string keyVersion = null);

        /// <summary>
        /// Sets a secret in keyvault
        /// </summary>
        /// <returns>The secret.</returns>
        /// <param name="secretName">Secret identifier.</param>
        /// <param name="value">The value to be stored.</param>
        Task<SecretBundle> SetSecretAsync(string secretName, string value);

        /// <summary>
        /// Stores a key pair into keyvault.
        /// </summary>
        /// <returns>The created key pair.</returns>
        /// <param name="identifier">key pair identifier.</param>
        /// <param name="key">The actual key pair.</param>
        Task<bool> StoreKeyPairAsync(string identifier, KeyPair key);

        /// <summary>
        /// Gets public key from keyvault.
        /// </summary>
        /// <returns>The public key.</returns>
        /// <param name="identifier">the key's Identifier.</param>
        Task<string> GetPublicKeyAsync(string identifier);

        /// <summary>
        /// Gets private key from keyvault.
        /// </summary>
        /// <returns>The private key.</returns>
        /// <param name="identifier">Identifier.</param>
        Task<string> GetPrivateKeyAsync(string identifier);

        /// <summary>
        /// Encrypts the provided value
        /// </summary>
        /// <param name="keyIdentifier">The key encryption identifier in Azure Key Vault</param>
        /// <param name="algorithm">The ecryption transcation</param>
        /// <param name="value">The value to encrypt</param>
        /// <returns>The encrypted value</returns>
        Task<KeyOperationResult> EncryptAsync(string keyIdentifier, string algorithm, byte[] value);

        /// <summary>
        /// Decrypts the provided value
        /// </summary>
        /// <param name="keyIdentifier">The key encryption identifier in Azure Key Vault</param>
        /// <param name="algorithm">The ecryption transcation</param>
        /// <param name="value">The value to decrypt</param>
        /// <returns>The decrypted value</returns>
        Task<KeyOperationResult> DecryptAsync(string keyIdentifier, string algorithm, byte[] value);
    }
}

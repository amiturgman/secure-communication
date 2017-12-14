using System.Threading.Tasks;
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
        /// Gets the specified key (both public and private portions)
        /// </summary>
        /// <returns>The key.</returns>
        /// <param name="keyName">Key identifier.</param>
        /// <param name="keyVersion">Key version.</param>
        Task<KeyBundle> GetKeyAsync(string keyName, string keyVersion = null);

        /// <summary>
        /// Gets the specified secret
        /// </summary>
        /// <returns>The secret</returns>
        /// <param name="secretName">Secret identifier</param>
        Task<SecretBundle> GetSecretAsync(string secretName);

        /// <summary>
        /// Sets a secret in keyvault
        /// </summary>
        /// <returns>The secret.</returns>
        /// <param name="secretName">Secret identifier.</param>
        /// <param name="value">The value to be stored.</param>
        Task<SecretBundle> SetSecretAsync(string secretName, string value);

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

        /// <summary>
        /// Signs the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="keyIdentifier">Key identifier.</param>
        /// <param name="algorithm">Algorithm.</param>
        /// <param name="digest">Digest.</param>
        Task<KeyOperationResult> SignAsync(string keyIdentifier, string algorithm, byte[] digest);

        /// <summary>
        /// Verifies the async.
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="keyIdentifier">Key identifier.</param>
        /// <param name="algorithm">Algorithm.</param>
        /// <param name="digest">Digest.</param>
        /// <param name="signature">Signature.</param>
        Task<bool> VerifyAsync(string keyIdentifier, string algorithm, byte[] digest, byte[] signature);
    }
}

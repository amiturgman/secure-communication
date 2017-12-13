using System.Threading.Tasks;

namespace Contracts
{
    public interface IEncryptionManager
    {
        /// <summary>
        /// Encrypt the specified data.
        /// </summary>
        /// <returns>Encrypted data</returns>
        /// <param name="data">Data to be encrypted.</param>
        Task<byte[]> Encrypt(byte[] data);

        /// <summary>
        /// Decrypt the specified encryptedData.
        /// </summary>
        /// <returns>The decrypted data</returns>
        /// <param name="encryptedData">Encrypted data.</param>
        Task<byte[]> Decrypt(byte[] encryptedData);

        /// <summary>
        /// Sign the specified data.
        /// </summary>
        /// <returns>The signature</returns>
        /// <param name="data">The data to be signed</param>
        Task<byte[]> SignAsync(byte[] data);

        /// <summary>
        /// Verify the specified signature and data.
        /// </summary>
        /// <returns>The verify.</returns>
        /// <param name="signature">The signature for verify</param>
        /// <param name="data">The data which match the signature</param>
        Task<bool> VerifyAsync(byte[] data, byte[] signature);
    }
}

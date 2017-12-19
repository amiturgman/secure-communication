namespace Contracts
{
    /// <summary>
    /// Provides the interface for all of the cryptographic operations which we
    /// need as part of the communication pipeline
    /// </summary>
    public interface IEncryptionManager
    {
        /// <summary>
        /// Encrypt the specified data.
        /// </summary>
        /// <returns>Encrypted data</returns>
        /// <param name="data">Data to be encrypted.</param>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypt the specified encryptedData.
        /// </summary>
        /// <returns>The decrypted data</returns>
        /// <param name="encryptedData">Encrypted data.</param>
        byte[] Decrypt(byte[] encryptedData);

        /// <summary>
        /// Sign the specified data.
        /// </summary>
        /// <returns>The signature</returns>
        /// <param name="data">The data to be signed</param>
        byte[] Sign(byte[] data);

        /// <summary>
        /// Verify the specified signature and data.
        /// </summary>
        /// <returns>The verify.</returns>
        /// <param name="signature">The signature for verify</param>
        /// <param name="data">The data which match the signature</param>
        bool Verify(byte[] data, byte[] signature);
    }
}

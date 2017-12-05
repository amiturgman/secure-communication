using System.Threading.Tasks;

public interface ISecretsManagement
{
    /// <summary>
    /// Encrypt the specified data.
    /// </summary>
    /// <returns>Encrypted data</returns>
    /// <param name="data">Data to be encrypted.</param>
    Task<string> Encrypt(string data);

    /// <summary>
    /// Decrypt the specified encryptedData.
    /// </summary>
    /// <returns>The decrypted data</returns>
    /// <param name="encryptedData">Encrypted data.</param>
    Task<string> Decrypt(string encryptedData);

    /// <summary>
    /// Sign the specified data.
    /// </summary>
    /// <returns>The signature</returns>
    /// <param name="data">The data to be signed</param>
    Task<byte[]> Sign(string data);

    /// <summary>
    /// Verify the specified signature and data.
    /// </summary>
    /// <returns>The verify.</returns>
    /// <param name="signature">The signature for verify</param>
    /// <param name="data">The data which match the signature</param>
    Task<bool> Verify(byte[] signature, string data);
}

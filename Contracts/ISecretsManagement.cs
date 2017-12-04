using System.Threading.Tasks;

public interface ISecretsManagement
{
    Task<string> Encrypt(string data);

    Task<string> Decrypt(string encryptedData);

    Task<byte[]> Sign(string data);

    Task<bool> Verify(byte[] signature, string data);
}

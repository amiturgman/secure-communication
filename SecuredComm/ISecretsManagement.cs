using System;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public interface ISecretsManagement
    {
        // todo: fill with a wrapper for our keys management mechanism
        Task<string> Encrypt(string keyName, string data);

        Task<string> Decrypt(string keyName, string encryptedData);

        Task<string> Sign(string keyName, string data);

        Task<bool> Verify(string keyName, string signature);
    }
}

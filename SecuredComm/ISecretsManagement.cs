using System;
using System.Threading.Tasks;

namespace SecuredComm
{
    public interface ISecretsManagement
    {
        // todo: fill with a wrapper for our keys management mechanism
        Task<string> Encrypt(string keyName, string data);

        Task<string> Decrypt(string keyName, string encData);

        Task<string> Sign(string keyName, string data);

        Task<string> Verify();
    }
}

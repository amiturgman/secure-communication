using System;
using System.Threading.Tasks;
using Contracts;

namespace SecuredCommTests
{
    public class SecretsManagementMock : ISecretsManagement
    {
        Task<string> ISecretsManagement.Decrypt(byte[] encryptedData)
        {
            throw new NotImplementedException();
        }

        Task<byte[]> ISecretsManagement.Encrypt(string data)
        {
            throw new NotImplementedException();
        }

        Task<byte[]> ISecretsManagement.Sign(string data)
        {
            throw new NotImplementedException();
        }

        Task<bool> ISecretsManagement.Verify(byte[] signature, string data)
        {
            throw new NotImplementedException();
        }
    }
}

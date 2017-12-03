using System;
using SecuredCommunication;
using Xunit;

namespace SecretsManagementTests
{
    public class UnitTest1
    {
        [Fact]
        public void Sanity_VerifyCanBeCreated()
        {
            var kvInfo = new KeyVaultInfoMock("http://dummyKvUri");
            var secMgmnt = new SecretsManagement(kvInfo);
        }

        [Fact]
        public async void Sanity_Encryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultInfoMock(kvUri);
            var secMgmnt = new SecretsManagement(kvInfo);
            var encryptedData = await secMgmnt.Encrypt(kvUri, "encKey", rawData);

            Assert.NotEqual(encryptedData, rawData);
        }

        [Fact]
        public async void Sanity_Decryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultInfoMock(kvUri);
            var secMgmnt = new SecretsManagement(kvInfo);

            // Encrypt
            var encryptedData = await secMgmnt.Encrypt(kvUri, "encKey", rawData);

            // Decrypt
            var decryptedData = await secMgmnt.Decrypt(kvUri, "encKey", encryptedData);

            // Verify the process ended succesfully and the data is plain text
            Assert.NotEqual(encryptedData, rawData);
            Assert.Equal(decryptedData, rawData);
        }
    }
}

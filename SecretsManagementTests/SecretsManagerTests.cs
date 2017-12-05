using System;
using SecuredCommunication;
using Xunit;

namespace SecretsManagementTests
{
    public class SecretsManagerTests
    {
        [Fact]
        public void Sanity_VerifyCanBeCreated()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var secretsMgmnt = new SecretsManagement("enc", "dec", "sign", "verify", kvInfo, kvInfo);
        }

        [Fact]
        public async void Sanity_Encryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new SecretsManagement("enc_public", "dec_private", "sign", "verify", kvInfo, kvInfo);
            var encryptedData = await secretsMgmnt.Encrypt(rawData);

            Assert.NotEqual(encryptedData, rawData);
        }

        [Fact]
        public async void Sanity_Decryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new SecretsManagement("enc_public", "dec_private", "sign", "verify", kvInfo, kvInfo);

            // Encrypt
            var encryptedData = await secretsMgmnt.Encrypt(rawData);

            // Decrypt
            var decryptedData = await secretsMgmnt.Decrypt(encryptedData);

            // Verify the process ended succesfully and the data is plain text
            Assert.NotEqual(encryptedData, rawData);
            Assert.Equal(decryptedData, rawData);
            Assert.Equal(encryptedData.Substring(encryptedData.Length - 2, 2), "==");
        }

        [Fact]
        public async void Sanity_Sign()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new SecretsManagement("enc_public", "dec_private", "sign", "verify", kvInfo, kvInfo);

            // Sign the data
            var signature = await secretsMgmnt.Sign(rawData);

            // todo: check what the actual expected signature length
            Assert.Equal(signature.Length, 256);
        }

        [Fact]
        public async void Sanity_Verify()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new SecretsManagement("enc_public", "dec_private", "sign", "verify", kvInfo, kvInfo);

            // Sign the data
            var signature = await secretsMgmnt.Sign(rawData);

            // todo: check what the actual expected signature length
            Assert.True(await secretsMgmnt.Verify(signature, rawData));
        }
    }
}

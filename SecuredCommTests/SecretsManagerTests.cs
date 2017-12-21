using Contracts;
using SecuredCommunication;
using Xunit;

namespace UnitTests
{
    public class SecretsManagerTests
    {
        private const string c_encKeyName = "enc_public";
        private const string c_decKeyName = "dec_private";
        private const string c_signKeyName = "sign_private";
        private const string c_verifyKeyName = "verify_public";

        [Fact]
        public void Sanity_VerifyCanBeCreated()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var secretsMgmnt = new KeyVaultSecretManager(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            secretsMgmnt.Initialize().Wait();
        }

        [Fact]
        public async void Sanity_Encryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new KeyVaultSecretManager(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            await secretsMgmnt.Initialize();

            var encryptedData = secretsMgmnt.Encrypt(Utils.ToByteArray(rawData));


            Assert.IsType<byte[]>(encryptedData);
        }

        [Fact]
        public async void Sanity_Decryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new KeyVaultSecretManager(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            await secretsMgmnt.Initialize();

            // Encrypt
            var encryptedData = secretsMgmnt.Encrypt(Utils.ToByteArray(rawData));

            // Decrypt
            var decryptedData = secretsMgmnt.Decrypt(encryptedData);

            // Verify the process ended successfully and the data is plain text
            Assert.IsType<byte[]>(encryptedData);
            Assert.Equal(256, encryptedData.Length);
            Assert.Equal(decryptedData, Utils.ToByteArray(rawData));
        }

        [Fact]
        public async void Sanity_Sign()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new KeyVaultSecretManager(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            await secretsMgmnt.Initialize();

            // Sign the data
            var signature = secretsMgmnt.Sign(Utils.ToByteArray(rawData));

            Assert.Equal(signature.Length, 256);
        }

        [Fact]
        public async void Sanity_Verify()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new KeyVaultMock(kvUri);
            var secretsMgmnt = new KeyVaultSecretManager(c_encKeyName, c_decKeyName, c_signKeyName, c_verifyKeyName, kvInfo, kvInfo);
            await secretsMgmnt.Initialize();

            // Sign the data
            var signature = secretsMgmnt.Sign(Utils.ToByteArray(rawData));

            Assert.True(secretsMgmnt.Verify(Utils.ToByteArray(rawData), signature));
        }
    }
}

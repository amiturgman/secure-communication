using Wallet.Communication;
using Wallet.Cryptography;
using Xunit;
using static Wallet.Cryptography.KeyVaultCryptoActions;

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
            var kvInfo = new DatabaseMock("http://dummyKvUri");
            var secretsMgmnt = new KeyVaultCryptoActions(
                new CertificateInfo(c_encKeyName, string.Empty),
                new CertificateInfo(c_decKeyName, string.Empty),
                new CertificateInfo(c_signKeyName, string.Empty),
                new CertificateInfo(c_verifyKeyName, string.Empty),
                kvInfo,
                kvInfo);
            secretsMgmnt.Initialize().Wait();
        }

        [Fact]
        public async void Sanity_Encryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new DatabaseMock(kvUri);
            var secretsMgmnt = new KeyVaultCryptoActions(
                new CertificateInfo(c_encKeyName, string.Empty),
                new CertificateInfo(c_decKeyName, string.Empty),
                new CertificateInfo(c_signKeyName, string.Empty),
                new CertificateInfo(c_verifyKeyName, string.Empty),
                kvInfo,
                kvInfo);
            await secretsMgmnt.Initialize();

            var encryptedData = secretsMgmnt.Encrypt(Utils.ToByteArray(rawData));


            Assert.IsType<byte[]>(encryptedData);
        }

        [Fact]
        public async void Sanity_Decryption()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new DatabaseMock(kvUri);
            var secretsMgmnt = new KeyVaultCryptoActions(
                new CertificateInfo(c_encKeyName, string.Empty),
                new CertificateInfo(c_decKeyName, string.Empty),
                new CertificateInfo(c_signKeyName, string.Empty),
                new CertificateInfo(c_verifyKeyName, string.Empty),
                kvInfo,
                kvInfo);
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
            var kvInfo = new DatabaseMock(kvUri);
            var secretsMgmnt = new KeyVaultCryptoActions(
                new CertificateInfo(c_encKeyName, string.Empty),
                new CertificateInfo(c_decKeyName, string.Empty),
                new CertificateInfo(c_signKeyName, string.Empty),
                new CertificateInfo(c_verifyKeyName, string.Empty),
                kvInfo,
                kvInfo);
            await secretsMgmnt.Initialize();

            // Sign the data
            var signature = secretsMgmnt.Sign(Utils.ToByteArray(rawData));

            Assert.Equal(256, signature.Length);
        }

        [Fact]
        public async void Sanity_Verify()
        {
            var kvUri = "http://dummyKvUri";
            var rawData = "Some data !!!";
            var kvInfo = new DatabaseMock(kvUri);
            var secretsMgmnt = new KeyVaultCryptoActions(
                new CertificateInfo(c_encKeyName, string.Empty),
                new CertificateInfo(c_decKeyName, string.Empty),
                new CertificateInfo(c_signKeyName, string.Empty),
                new CertificateInfo(c_verifyKeyName, string.Empty),
                kvInfo,
                kvInfo);
            await secretsMgmnt.Initialize();

            // Sign the data
            var signature = secretsMgmnt.Sign(Utils.ToByteArray(rawData));

            Assert.True(secretsMgmnt.Verify(Utils.ToByteArray(rawData), signature));
        }
    }
}
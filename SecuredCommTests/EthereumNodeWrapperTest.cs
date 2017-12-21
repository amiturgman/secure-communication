using SecuredCommunication;
using Xunit;

namespace UnitTests
{
    public class EthereumNodeWrapperTest
    {
        [Fact]
        public async void Sanity_Sign_Transaction()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var ethereumWallet = new EthereumNodeWrapper(kvInfo, "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE");
            var transactionHash = await ethereumWallet.SignTransactionAsync("sender", TestConstants.publicKey, 10000);

            Assert.Equal(208, transactionHash.Length);
        }

        [Fact]
        public async void Sanity_Get_Balance()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var ethereumWallet = new EthereumNodeWrapper(kvInfo, "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE");
            var transactionHash = await ethereumWallet.GetCurrentBalance(TestConstants.publicKey);

            Assert.IsType<decimal>(transactionHash);
        }

        [Fact]
        public void Test_Create_account_returns_KeyPair_As_Expected()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var ethereumWallet = new EthereumNodeWrapper(kvInfo, "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE");
            var accountKeyPair = ethereumWallet.CreateAccount();

            Assert.Equal(42, accountKeyPair.GetPublicAddress().Length);
            Assert.True(accountKeyPair.GetPublicAddress().StartsWith("0x"));
            Assert.Equal(66, accountKeyPair.GetPrivateKey().Length);
            Assert.True(accountKeyPair.GetPrivateKey().StartsWith("0x"));
        }

        [Fact]
        public async void Test_SendTransaction()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var ethereumWallet = new EthereumNodeWrapper(kvInfo, "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE");
            var transactionHash = await 
                ethereumWallet.SignTransactionAsync("sender", TestConstants.publicKey, 100);
            var transactionResult = await ethereumWallet.SendRawTransactionAsync(transactionHash);

            Assert.True(transactionResult.StartsWith("0x"));
            Assert.Equal(66, transactionResult.Length);
        }
    }
}

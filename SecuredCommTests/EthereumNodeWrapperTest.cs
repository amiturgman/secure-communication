using SecuredCommunication;
using Xunit;

namespace SecuredCommTests
{
    public class EthereumNodeWrapperTest
    {
        [Fact]
        public async void Sanity_Sign_Transaction()
        {
            var kvInfo = new KeyVaultMock("http://dummyKvUri");
            var ethereumWallet = new EthereumNodeWrapper(kvInfo, "https://rinkeby.infura.io/fIF86MY6m3PHewhhJ0yE");
            var transactionHash = await ethereumWallet.SignTransaction("sender", TestConstants.publicKey, 10000);

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
    }
}

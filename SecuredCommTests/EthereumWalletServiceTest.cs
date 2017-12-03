using SecuredCommunication;
using Xunit;

namespace SecuredCommTests
{
    public class EthereumWalletServiceTest
    {
        [Fact]
        public async void Sanity_Sign_Transaction()
        {
            var secretManagmentMock = new SecretsManagementMock();
            var ethereumWallet = new EthereumWalletService("http://someurl", secretManagmentMock);
            var trnsactionHash = await ethereumWallet.SignTransaction("sender", TestConstants.publicKey, 10000);

            Assert.Equal(208, trnsactionHash.Length);
        }

        [Fact]
        public async void Sanity_Get_Balance()
        {
            var trnsactionHash = await EthereumWalletService.GetCurrentBalance(TestConstants.publicKey);

            Assert.IsType<decimal>(trnsactionHash);
        }
    }
}

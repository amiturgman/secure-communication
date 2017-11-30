using Nethereum.Signer;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class EthereumNodeWrapper : IBlockchainNodeWrapper
    {
        // Create account
        public async Task<KeyPair> GenerateAccount()
        {
            var ecKey = EthECKey.GenerateKey();

            return await Task.FromResult(new KeyPair(ecKey.GetPublicAddress(), ecKey.GetPrivateKey()));
        }

        Task<bool> IBlockchainNodeWrapper.SendTransaction(string hash)
        {
            throw new System.NotImplementedException();
        }
    }
}

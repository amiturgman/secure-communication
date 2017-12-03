using Nethereum.JsonRpc.IpcClient;
using Nethereum.Signer;
using Nethereum.Web3;
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

        public async Task<string> SendTransaction(string hash)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var transactionResult = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(hash);
            return transactionResult;
        }
    }
}

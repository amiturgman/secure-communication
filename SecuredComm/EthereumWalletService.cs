using Nethereum.JsonRpc.IpcClient;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class EthereumWalletService : IWalletService
    {
        private ISecretsManagement secretsManagement;
        private string keyVaultUrl;

        #region Public Methods
        public EthereumWalletService(string keyVaultUrl, ISecretsManagement secretsManagement)
        {
            this.keyVaultUrl = keyVaultUrl;
            this.secretsManagement = secretsManagement;
        }

        public async Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);

            var senderKeyPair = await LoadKeyPairFromKeyVault(senderIdentifier);
            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderKeyPair.PublicKey);
            var transactionHash = Web3.OfflineTransactionSigner.SignTransaction(senderKeyPair.PrivateKey, recieverAddress, amount, txCount.Value);

            return await Task.FromResult(transactionHash);
        }

        public static async Task<decimal> GetCurrentBalance(string address)
        {
            var client = new IpcClient("geth.ipc");
            var web3 = new Web3(client);
            var unitConverion = new Nethereum.Util.UnitConversion();
            var currentBalance = unitConverion.FromWei(await web3.Eth.GetBalance.SendRequestAsync(address));
            return currentBalance;
        }
        #endregion

        #region Private Methods
        private async Task<KeyPair> LoadKeyPairFromKeyVault(string identifier)
        {
            var publicKey = await secretsManagement.GetPublicKey(keyVaultUrl, identifier);
            var privateKey = await secretsManagement.GetPrivateKey(keyVaultUrl, identifier);

            return new KeyPair(publicKey, privateKey);
        }

        #endregion
    }
}

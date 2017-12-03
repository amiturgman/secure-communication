using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class EthereumWalletService : IWalletService
    {
        private KeyVault KeyVault;
        private SecretsManagement secretsManagement;
        private string keyVaultUrl;
        #region Public Methods
        public EthereumWalletService(string keyVaultUrl)
        {
            this.keyVaultUrl = keyVaultUrl;
            KeyVault = new KeyVault(keyVaultUrl);
            secretsManagement = new SecretsManagement(KeyVault);
        }

        public async Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount)
        {
            var web3 = new Web3();

            var senderKeyPair = await LoadKeyPairFromKeyVault(senderIdentifier);
            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderKeyPair.PublicKey);
            var transactionHash = Web3.OfflineTransactionSigner.SignTransaction(senderKeyPair.PrivateKey, recieverAddress, amount, txCount.Value);

            return await Task.FromResult(transactionHash);
        }

        public static async Task<decimal> GetCurrentBalance(Account account)
        {
            var web3 = new Web3();
            var unitConverion = new Nethereum.Util.UnitConversion();
            var currentBalance = unitConverion.FromWei(await web3.Eth.GetBalance.SendRequestAsync(account.Address));
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

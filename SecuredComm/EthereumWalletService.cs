using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public class EthereumWalletService : IWalletService
    {
        private KeyVaultInfo KeyVault;
        private const string KeyVaultName = "EthereumWallet";
        private SecretsManagement secretsManagement;

        #region Public Methods
        public EthereumWalletService()
        {
            KeyVault = new KeyVaultInfo(KeyVaultName);
            secretsManagement = new SecretsManagement(KeyVault);
        }

        public async Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount, BigInteger nonce)
        {
            var web3 = new Web3();

            var senderKeyPair = await LoadKeyPairFromKeyVault(senderIdentifier);
            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(senderKeyPair.PublicKey);
            var transactionHash = Web3.OfflineTransactionSigner.SignTransaction(senderKeyPair.PrivateKey, recieverAddress, amount, txCount.Value);

            return await Task.FromResult(transactionHash);
        }
        #endregion

        #region Private Methods
        private async Task<KeyPair> LoadKeyPairFromKeyVault(string identifier)
        {
            var publicKey = await secretsManagement.GetPublicKey(KeyVaultName, identifier);
            var privateKey = await secretsManagement.GetPrivateKey(KeyVaultName, identifier);

            return new KeyPair(publicKey, privateKey);
        }

        #endregion
    }
}

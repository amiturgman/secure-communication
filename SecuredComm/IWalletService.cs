using System.Numerics;
using System.Threading.Tasks;

namespace SecuredCommunication
{
    public interface IWalletService
    {
        // Sign Transaction
        Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount, BigInteger nonce);
    }
}

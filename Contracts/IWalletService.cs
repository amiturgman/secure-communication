using System.Numerics;
using System.Threading.Tasks;

public interface IWalletService
{
    // Sign Transaction
    Task<string> SignTransaction(string senderIdentifier, string recieverAddress, BigInteger amount);
}

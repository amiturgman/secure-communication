using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

public interface IKeyVaultInfo
{
    string GetUrl();

    Task<KeyBundle> GetKeyAsync(
                                string vault,
                                string keyName,
                                string keyVersion = null);
}

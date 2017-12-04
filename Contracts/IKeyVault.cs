using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
public interface IKeyVault
{
    string GetUrl();

    Task<SecretBundle> GetSecretAsync(string vault, string secretName);

    Task<KeyBundle> GetKeyAsync(string vault,
                                string keyName,
                                string keyVersion = null);

    Task<SecretBundle> SetSecretAsync(string vault, string secretName, string value);
}

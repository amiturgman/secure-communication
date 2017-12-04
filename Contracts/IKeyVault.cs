using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
public interface IKeyVault
{
    string GetUrl();

    Task<SecretBundle> GetSecretAsync(string secretName);

    Task<KeyBundle> GetKeyAsync(string keyName,
                                string keyVersion = null);

    Task<SecretBundle> SetSecretAsync(string secretName, string value);

    Task<bool> StoreKeyPair(string identifier, KeyPair key);

    Task<string> GetPublicKey(string identifier);

    Task<string> GetPrivateKey(string identifier);
}

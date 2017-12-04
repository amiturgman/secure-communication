using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;
using System.Security.Cryptography;

public class KeyVaultMock : IKeyVault
{
    private string kvUri;

    public KeyVaultMock(string kvUri)
    {
        this.kvUri = kvUri;
    }

    public Task<SecretBundle> GetSecretAsync(
   string vault,
           string secretName)
    {
        throw new Exception();
    }

    public Task<SecretBundle> SetSecretAsync(string vault, string secretName, string value)
    {
        throw new Exception();
    }


    Task<KeyBundle> IKeyVault.GetKeyAsync(string vault, string keyName, string keyVersion)
    {

        var x = new X509Certificate2("../../../testCert.pfx", "abc123ABC", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

        var key = keyName.Contains("private") ? x.GetRSAPrivateKey() : x.GetRSAPublicKey();
        using (RSA rsa = key)
        {
            var shouldGetPrivate = keyName.Contains("private");
            var parameters = rsa.ExportParameters(shouldGetPrivate);
            KeyBundle bundle = new KeyBundle
            {
                Key = new Microsoft.Azure.KeyVault.WebKey.JsonWebKey
                {
                    Kty = Microsoft.Azure.KeyVault.WebKey.JsonWebKeyType.Rsa,
                    // Private stuff
                    D = parameters.D,
                    DP = parameters.DP,
                    DQ = parameters.DQ,
                    P = parameters.P,
                    Q = parameters.Q,
                    QI = parameters.InverseQ,
                    // Public stuff
                    N = parameters.Modulus,
                    E = parameters.Exponent,
                },
            };
            return Task.FromResult(bundle);
        }
    }

    string IKeyVault.GetUrl()
    {
        return this.kvUri;
    }
}

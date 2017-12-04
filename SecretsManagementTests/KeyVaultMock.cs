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

    public Task<string> GetPrivateKey(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetPublicKey(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task<SecretBundle> GetSecretAsync(string secretName)
    {
        throw new Exception();
    }

    public string GetUrl()
    {
        throw new NotImplementedException();
    }

    public Task<SecretBundle> SetSecretAsync(string secretName, string value)
    {
        throw new Exception();
    }

    public Task<bool> StoreKeyPair(string identifier, KeyPair key)
    {
        throw new NotImplementedException();
    }

    Task<KeyBundle> IKeyVault.GetKeyAsync(string keyName, string keyVersion)
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
}

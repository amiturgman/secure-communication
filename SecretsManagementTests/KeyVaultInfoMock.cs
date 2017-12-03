using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault.Models;

    public class KeyVaultInfoMock : IKeyVaultInfo
    {
        private string kvUri;

        public KeyVaultInfoMock(string kvUri)
        {
            this.kvUri = kvUri;
        }

    Task<KeyBundle> IKeyVaultInfo.GetKeyAsync(string vault, string keyName, string keyVersion)
    {
        X509Certificate2 x = new X509Certificate2();

        if (keyName == "private") {
            x.Import("MyTestCertForUTs_privateKey.p12");
            return x.PrivateKey;
        } else {
            x.Import("MyTestCertForUTs_publicKey.pem");
            return x.PublicKey;
        }
        var kb = new KeyBundle();
        kb.
        // return hardcoded key
        throw new NotImplementedException();
    }

    string IKeyVaultInfo.GetUrl()
    {
        return this.kvUri;
    }
}

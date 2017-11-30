namespace SecuredCommunication
{
    public class KeyVaultInfo
    {
        public string KvName;
        public string AppId;
        public string ServicePrincipalId;

        public KeyVaultInfo(string kvName, string appId, string principalId)
        {
            KvName = kvName;
            AppId = appId;
            ServicePrincipalId = principalId;
        }
    }
}

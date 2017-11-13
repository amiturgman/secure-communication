using System;
using System.Threading.Tasks;

interface ISecuredComm
{
    // the key is securly stored on keyvault or alike and accessed via the secrets management
    Task SendEncryptedMsgAsync(string encryptionKeyName, string queueName, string msg);

    Task SendUnencryptedMsgAsync(string queueName, string msg);

    string ListenOnEncryptedQueue(string decryptionKeyName, string queueName, Action<string> cb);

    string ListenOnUnencryptedQueue(string queueName, Action<string> cb);

    void CancelListeningOnQueue(string consumerTag);
}
using System;
using System.Threading.Tasks;

[Serializable]
public class Message
{
    public Message(string msg){
        data = msg;
    }

    public bool isEncrypted;
    public string data;
    public string sign;
}

interface ISecuredComm
{
    /// <summary>
    /// Encrypts and Sends the a message
    /// </summary>
    /// <param name="encryptionKeyName"> the key is securly stored on keyvault or alike and accessed via the secrets management</param>
    /// <param name="queueName">Queue name.</param>
    /// <param name="msg">Message.</param>
    Task SendEncryptedMsgAsync(string encryptionKeyName, string signingKeyName, string queueName, Message msg);

    /// <summary>
    /// Sends an unencrypted message.
    /// </summary>
    /// <param name="queueName">Queue name.</param>
    /// <param name="msg">Message.</param>
    Task SendUnencryptedMsgAsync(string signingKeyName, string queueName, Message msg);

    /// <summary>
    /// Creates a listener on a queue where messages are encrypted. The message's data is automatically decrypted
    /// </summary>
    /// <returns>The consumer tag</returns>
    string ListenOnEncryptedQueue(string decryptionKeyName, string verificationKeyName, string queueName, Action<Message> cb);

    /// <summary>
    /// Creates a listener on a queue where messages are unencrypted
    /// </summary>
    /// <returns>The consumer tag</returns>
    string ListenOnUnencryptedQueue(string verificationKeyName, string queueName, Action<Message> cb);

    /// <summary>
    /// Cancels a specific listener
    /// </summary>
    /// <param name="consumerTag">The listener's id</param>
    void CancelListeningOnQueue(string consumerTag);
}
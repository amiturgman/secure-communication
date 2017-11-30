using System;
using System.Threading.Tasks;

[Serializable]
public class Message
{
    public Message(string msg){
        data = msg;
    }

    public bool isEncrypted;
    public bool isSigned;
    public string data;
    public string sign;

    // used to let the listener know which cert is for verification.
    // If the verification passed then we also know that 
    public string verificationKeyName;
}

interface ISecuredComm
{
    /// <summary>
    /// Encrypts and Sends the a message
    /// </summary>
    /// <param name="encryptionKeyName"> the key is securly stored on keyvault or alike and accessed via the secrets management</param>
    /// <param name="queueName">Queue name.</param>
    /// <param name="msg">Message.</param>
    Task SendEncryptedMsgAsync(string encryptionKeyName, string signingKeyName, string queueName, string topic, Message msg);

    /// <summary>
    /// Sends an unencrypted message.
    /// </summary>
    /// <param name="queueName">Queue name.</param>
    /// <param name="msg">Message.</param>
    Task SendUnencryptedMsgAsync(string signingKeyName, string queueName, string topic, Message msg);

    /// <summary>
    /// Creates a listener on a queue where messages are encrypted. The message's data is automatically decrypted
    /// </summary>
    /// <returns>The consumer tag</returns>
    string ListenOnQueue(string verificationKeyName, string queueName, string[] topics, Action<Message> cb, string decryptionKeyName = "");

    /// <summary>
    /// Cancels a specific listener
    /// </summary>
    /// <param name="consumerTag">The listener's id</param>
    void CancelListeningOnQueue(string consumerTag);
}
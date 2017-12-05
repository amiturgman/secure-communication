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
    public byte[] sign;

    // used to let the listener know which cert is for verification.
    // If the verification passed then we also know that 
    public string verificationKeyName;
}

public interface ISecuredComm
{
    /// <summary>
    /// Enqueue a message to the queue
    /// </summary>
    /// <param name="queue">Queue name.</param>
    /// <param name="msg">Message.</param>
    Task EnqueueAsync(string queue, Message msg);

    /// <summary>
    /// Creates a listener on a queue where messages are encrypted. The message's data is automatically decrypted
    /// </summary>
    /// <returns>The consumer tag</returns>
    string Dequeue(string queueName, Action<Message> cb);
}
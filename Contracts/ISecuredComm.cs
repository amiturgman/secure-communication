using System;
using System.Threading.Tasks;

namespace Contracts
{
    [Serializable]
    public class Message
    {
        public bool IsEncrypted;
        public bool IsSigned;
        public byte[] Data;
        public byte[] Signature;
    }

    public interface ISecuredComm
    {
        /// <summary>
        /// Enqueue a message to the queue
        /// </summary>
        /// <param name="queue">Queue name.</param>
        /// <param name="msg">Message.</param>
        Task EnqueueAsync(string queue, string msg);

        /// <summary>
        /// Creates a listener on a queue where messages are encrypted. The message's data is automatically decrypted
        /// </summary>
        /// <returns>The consumer tag</returns>
        string Dequeue(string queueName, Action<Message> cb);
    }
}
using System;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ISecuredComm
    {
        /// <summary>
        /// Enqueue a message to the queue
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="msg">Message.</param>
        Task EnqueueAsync(string queueName, string msg);

        /// <summary>
        /// Creates a listener on a queue where messages are encrypted. The message's data is automatically decrypted
        /// </summary>
        /// <returns>The consumer tag</returns>
        Task<string> DequeueAsync(string queueName, Action<Message> cb);
    }
}
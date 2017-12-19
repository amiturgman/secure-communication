using System;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Interface for a queue based communication pipeline
    /// </summary>
    /// todo:IQueueManager?
    public interface IQueueCommunication
    {
        /// <summary>
        /// Enqueue a message to the queue
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="msg">Message.</param>
        Task EnqueueAsync(string queueName, string msg);

        /// <summary>
        /// Creates a listener on a queue.
        /// </summary>
        /// <returns>The listener's identifier</returns>
        /// <param name="queueName">The queue to listen on</param>
        /// <param name="cb">a callback to execute once a message arrives</param>
        Task<string> DequeueAsync(string queueName, Action<byte[]> cb);
    }
}
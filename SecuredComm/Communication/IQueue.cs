using System;
using System.Threading.Tasks;

namespace Communication
{
    /// <summary>
    /// Interface for a queue based communication pipeline
    /// </summary>
    public interface IQueue
    {
        /// <summary>
        /// Enqueue a message to the queue
        /// </summary>
        /// <param name="msg">Message.</param>
        Task EnqueueAsync(byte[] msg);

        /// <summary>
        /// Creates a listener on a queue.
        /// </summary>
        /// <returns>The listener's identifier</returns>
        /// <param name="cb">a callback to execute once a message arrives</param>
        Task<string> DequeueAsync(Action<byte[]> cb);

        /// <summary>
        /// Creates a listener on a queue.
        /// </summary>
        /// <returns>The listener's identifier</returns>
        /// <param name="cb">a callback to execute once a message arrives</param>
        Task DequeueAsync(Action<byte[]> cb, TimeSpan waitTime);
    }
}
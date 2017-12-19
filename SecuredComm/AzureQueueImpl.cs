using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Queue;

namespace SecuredCommunication
{
    public class AzureQueueImpl : IQueueCommunication
    {
        #region private members

        private readonly CloudQueueClient m_queueClient;
        private readonly IEncryptionManager m_secretMgmt;
        private readonly bool m_isEncrypted;
        private bool m_isCancelled;

        #endregion

        // todo: input sanity
        // iscanceled=> isactive
        public AzureQueueImpl(string connectionString, IEncryptionManager secretMgmnt, bool isEncrypted)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            m_queueClient = storageAccount.CreateCloudQueueClient();
            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
            m_isCancelled = false;
        }

        /// <summary>
        /// Enqueues a message, it will be automatically signed and if chosen (ctor) encrypted as well
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="queueName">Queue name.</param>
        /// <param name="msg">Message.</param>
        public async Task EnqueueAsync(string queueName, string msg)
        {
            var queue = m_queueClient.GetQueueReference(queueName);
            // todo: add init method that creates the queue...
            await queue.CreateIfNotExistsAsync();
            var message = 
                CloudQueueMessage.CreateCloudQueueMessageFromByteArray(
                    MessageUtils.CreateMessageForQueue(msg, m_secretMgmt, m_isEncrypted));
            await queue.AddMessageAsync(message);
            // todo: error handling.
        }

        /// <summary>
        /// Dequeues a message. The signature will be verified, in case of a verification failure an exception will be thrown.
        /// The callback recieves a single argument which is the decryted and verified message
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="queueName">Queue name.</param>
        /// <param name="cb">Callback</param>
        public async Task<string> DequeueAsync(string queueName, Action<byte[]> cb)
        {
            m_isCancelled = false;

            var queue = m_queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
           
            while (!m_isCancelled)
            {
                try
                {
                    var retrievedMessage = await queue.GetMessageAsync();
                    if (retrievedMessage != null)
                    {
                        MessageUtils.ProcessQueueMessage(retrievedMessage.AsBytes, m_secretMgmt, cb);
                        await queue.DeleteMessageAsync(retrievedMessage);

                        // no need to sleep, try again
                        continue;
                    }
                }
                catch(Exception exc) {
                    Console.WriteLine("Caught an exception: " + exc);
                    // Don't rethrow as we want the dequeue loop to continue
                }

                Thread.Sleep(3000);
            }

            // TODO: check if needed?
            return "success";
        }

        /// <summary>
        /// Stops the dequeuing process
        /// </summary>
        public void CancelListeningOnQueue()
        {
            m_isCancelled = true;
        }
    }
}
 
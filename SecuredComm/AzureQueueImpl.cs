using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Queue;
using SecuredComm;

namespace SecuredCommunication
{
    public class AzureQueueImpl : IQueueManager
    {
        #region private members

        private CloudQueue m_queue;
        private readonly IEncryptionManager m_secretMgmt;
        private readonly bool m_isEncrypted;
        private bool m_isActive;
        private string m_queueName;
        private string m_connectionString;
        private bool m_isInitialized;

        #endregion

        public AzureQueueImpl(string queueName, string connectionString, IEncryptionManager secretMgmnt, bool isEncrypted)
        {
            m_connectionString = connectionString;
            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
            m_isActive = false;
            m_queueName = queueName;
            m_isInitialized = false;
        }

        public async Task Initialize()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(m_connectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();

            m_queue = queueClient.GetQueueReference(m_queueName);
            await m_queue.CreateIfNotExistsAsync();

            m_isInitialized = true;
        }

        /// <summary>
        /// Enqueues a message, it will be automatically signed and if chosen (ctor) encrypted as well
        /// </summary>
        /// <param name="msg">Message.</param>
        public async Task EnqueueAsync(string msg)
        {
            ThrowIfNotInitialized();

            var messageInBytes = MessageUtils.CreateMessageForQueue(msg, m_secretMgmt, m_isEncrypted);
            var message = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(messageInBytes);

            try
            {
                await m_queue.AddMessageAsync(message);
            }
            catch (StorageException ex)
            {
                Console.WriteLine($"Exception was thrown when trying to push message to queue, exception: {ex.Message}");
                throw;
            }
        }

        public Task<string> DequeueAsync(Action<byte[]> cb)
        {
            throw new SecureCommunicationException("This method signature is not supported for the Azure Queue implementation");
        }

        /// <summary>
        /// Dequeues a message. The signature will be verified, in case of a verification failure an exception will be thrown.
        /// The callback receives a single argument which is the decrypted and verified message
        /// </summary>
        /// <param name="cb">Callback</param>
        public async Task DequeueAsync(Action<byte[]> cb, TimeSpan waitTime)
        {
            ThrowIfNotInitialized();

            m_isActive = true;
           
            while (m_isActive)
            {
                try
                {
                    var retrievedMessage = await m_queue.GetMessageAsync();
                    if (retrievedMessage != null)
                    {
                        MessageUtils.ProcessQueueMessage(retrievedMessage.AsBytes, m_secretMgmt, cb);
                        await m_queue.DeleteMessageAsync(retrievedMessage);

                        // no need to sleep, try again
                        continue;
                    }
                }
                catch(Exception exc) {
                    Console.WriteLine("Caught an exception: " + exc);
                    // Don't re-throw as we want the dequeue loop to continue
                }

                Thread.Sleep((int)waitTime.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Stops the dequeuing process
        /// </summary>
        public void CancelListeningOnQueue()
        {
            ThrowIfNotInitialized();

            m_isActive = false;
        }

        private void ThrowIfNotInitialized(){
            if (!m_isInitialized) {
                throw new SecureCommunicationException("Object was not initialized");
            }
        }
    }
}
 
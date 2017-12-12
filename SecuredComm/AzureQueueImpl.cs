﻿using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Queue;

namespace SecuredCommunication
{
    public class AzureQueueImpl : ISecuredComm
    {
        private CloudQueueClient queueClient;
        private ISecretsManagement m_secretMgmt;
        private bool m_isEncrypted;
        private bool m_isCancelled;

        public AzureQueueImpl(ISecretsManagement secretMgmnt, bool isEncrypted)
        {
            CloudStorageAccount storageAccount = 
                CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            queueClient = storageAccount.CreateCloudQueueClient();
            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
            m_isCancelled = false;
        }

        public async Task EnqueueAsync(string queueName, string msg)
        {
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var message = 
                CloudQueueMessage.CreateCloudQueueMessageFromByteArray(
                    await Message.CreateMessageForQueue(msg, m_secretMgmt, m_isEncrypted));
            await queue.AddMessageAsync(message);
        }

        public async Task<string> Dequeue(string queueName, Action<Message> cb)
        {
            m_isCancelled = false;

            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            while (!m_isCancelled)
            {
                try
                {
                    var retrievedMessage = await queue.GetMessageAsync();
                    if (retrievedMessage != null)
                    {
                        await Message.DecryptAndVerifyQueueMessage(retrievedMessage.AsBytes, m_secretMgmt, cb);
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
 
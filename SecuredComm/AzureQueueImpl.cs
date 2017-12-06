using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.WindowsAzure.Storage; 
using Microsoft.WindowsAzure.Storage.Queue;

namespace SecuredComm
{
    public class AzureQueueImpl : ISecuredComm
    {
        private CloudQueueClient queueClient;
        private ISecretsManagement m_secretMgmt;
        private bool m_isEncrypted;
        private bool m_isCancelled;

        public AzureQueueImpl(ISecretsManagement secretMgmnt, bool isEncrypted)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            queueClient = storageAccount.CreateCloudQueueClient();
            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
            m_isCancelled = false;
        }

        public async Task EnqueueAsync(string queueName, string msg)
        {
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var message = CloudQueueMessage.CreateCloudQueueMessageFromByteArray(await Message.CreateMessageForQueue(msg, m_secretMgmt, m_isEncrypted));
            await queue.AddMessageAsync(message);
        }

        public async Task<string> Dequeue(string queueName, Action<Message> cb)
        {
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            while (!m_isCancelled)
            {
                CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();
                await Message.DecryptAndVerifyQueueMessage(retrievedMessage.AsBytes, m_secretMgmt, cb);

                Thread.Sleep(3000);
            }

            // TODO: check if needed?
            return "success";
        }

        public void CancelListeningOnQueue()
        {
            m_isCancelled = true;
        }
    }
}
 
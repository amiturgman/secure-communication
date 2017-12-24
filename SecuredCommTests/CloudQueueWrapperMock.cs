using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using SecuredCommunication;

namespace UnitTests.Mocks
{
    public class CloudQueueWrapperMock : ICloudQueueWrapper
    {
        public List<byte[]> QueueList;

        public Task AddMessageAsync(CloudQueueMessage message)
        {
            QueueList.Add(Utils.FromByteArray<Message>(message.AsBytes).Data);
            return Task.FromResult("result");
        }

        public Task CreateIfNotExistsAsync()
        {
            QueueList = new List<byte[]>();
            return Task.FromResult("result");
        }

        public Task<CloudQueueMessage> GetMessageAsync(TimeSpan? visibilityTimeout, QueueRequestOptions options, OperationContext operationContext)
        {
            var message = QueueList[QueueList.Count - 1];
            

            return Task.FromResult(CloudQueueMessage.CreateCloudQueueMessageFromByteArray(message));
        }

        public Task DeleteMessageAsync(CloudQueueMessage message)
        {
            QueueList.RemoveAt(QueueList.Count - 1);
            return Task.FromResult("result");
        }
    }
}

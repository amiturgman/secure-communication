using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using UnitTests.Mocks;
using Xunit;
using Encryption;
using Communication;

namespace UnitTests
{
    public class AzureQueueImplTests
    {
        [Fact]
        public async Task Test_Exception_Is_Thrown_When_Initialize_Not_CalledAsync()
        {
            var queueMock = new CloudQueueClientWrapperMock();
            var azureQueue = new AzureQueueImpl("queueName", queueMock, new EncryptionManagerMock(), true);

            try
            {
                await azureQueue.EnqueueAsync("some message");
            }
            catch (SecureCommunicationException ex)
            {
                Assert.Equal("Object was not initialized", ex.Message);
            }
        }

        [Fact]
        public async Task Test_Enqueue_Message_Happy_flow()
        {
            // Init
            var queueMock = new CloudQueueClientWrapperMock();
            var keyVaultMock = new KeyVaultMock("url");
            var encryptionManager = new KeyVaultEncryption("emc", "emc", "emc", "emc", keyVaultMock, keyVaultMock);
            await encryptionManager.Initialize();

            var queueName = "queueName";
            var azureQueue = new AzureQueueImpl(queueName, queueMock, encryptionManager, true);
            await azureQueue.Initialize();

            // Enqueue message
            var msg = "new message";
            await azureQueue.EnqueueAsync(msg);

            var queueRefernce = queueMock.GetQueueReference(queueName);

            var result = await queueRefernce.GetMessageAsync(TimeSpan.FromSeconds(10),
                new QueueRequestOptions(), new OperationContext());

            var encryptedMessage = Utils.FromByteArray<Message>(result.AsBytes);
            // String is encrypted, check it value
            Assert.Equal(256, encryptedMessage.Data.Length);
        }

        [Fact]
        public async Task Test_AzureImpl_Enqueue_Dequeue()
        {
            // Init
            var queueMock = new CloudQueueClientWrapperMock();
            var keyVaultMock = new KeyVaultMock("url");
            var encryptionManager = new KeyVaultEncryption("emc", "emc", "emc", "emc", keyVaultMock, keyVaultMock);
            await encryptionManager.Initialize();

            var queueName = "queueName";
            var azureQueue = new AzureQueueImpl(queueName, queueMock, encryptionManager, true);
            await azureQueue.Initialize();

            // Enqueue Message
            var msg = "new message";
            await azureQueue.EnqueueAsync(msg);

            var task = azureQueue.DequeueAsync((decrypted) =>
            {
                // Verify that the decrypted message equals to the original
                Assert.Equal(msg, Utils.FromByteArray<string>(decrypted));

            }, TimeSpan.FromMilliseconds(1));

            Thread.Sleep(10000);
            azureQueue.CancelListeningOnQueue();

            await task;

            // Verify there are no messages in the poison queue
            var poisonQueue = queueMock.GetQueueReference($"{queueName}-poison");
            await poisonQueue.CreateIfNotExistsAsync();
            var poisonMessage = await poisonQueue.GetMessageAsync(TimeSpan.MaxValue, new QueueRequestOptions(), new OperationContext());

            Assert.Null(poisonMessage);
        }
    }
}
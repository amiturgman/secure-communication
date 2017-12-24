using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using SecuredCommunication;
using UnitTests.Mocks;
using Xunit;

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
                Assert.Equal(ex.Message, "Object was not initialized");
            }
        }

        [Fact]
        public async Task Test_Enqueue_Message_Happy_flow()
        {
            var queueMock = new CloudQueueClientWrapperMock();
            var keyVaultMock = new KeyVaultMock("url");
            var encryptionManager = new KeyVaultSecretManager("emc", "emc", "emc", "emc", keyVaultMock, keyVaultMock);
            await encryptionManager.Initialize();

            var azureQueue = new AzureQueueImpl("queueName", queueMock, encryptionManager, true);
            await azureQueue.Initialize();

            var msg = "new message";
            await azureQueue.EnqueueAsync(msg);

            var queueRefernce = queueMock.GetQueueReference("some name");

            var result = await queueRefernce.GetMessageAsync(TimeSpan.FromSeconds(10),
                        new QueueRequestOptions(), new OperationContext());

            // String is encrypted, check it value
            Assert.Equal(256, result.AsBytes.Length);
        }

        [Fact]
        public async Task Test_AzureImpl_Enqueue_Dequeue()
        {
            var queueMock = new CloudQueueClientWrapperMock();
            var keyVaultMock = new KeyVaultMock("url");
            var encryptionManager = new KeyVaultSecretManager("emc", "emc", "emc", "emc", keyVaultMock, keyVaultMock);
            await encryptionManager.Initialize();

            var azureQueue = new AzureQueueImpl("queueName", queueMock, encryptionManager, true);
            await azureQueue.Initialize();

            var msg = "new message";
            await azureQueue.EnqueueAsync(msg);

            var queueRefernce = queueMock.GetQueueReference("some name");

            var result = await queueRefernce.GetMessageAsync(TimeSpan.FromSeconds(10),
                        new QueueRequestOptions(), new OperationContext());


            var task = azureQueue.DequeueAsync((decrypted) =>
            {
                Assert.NotEqual(msg, Utils.FromByteArray<string>(decrypted));
                
            }, TimeSpan.FromMilliseconds(1));

            Thread.Sleep(10000);
            azureQueue.CancelListeningOnQueue();

            await task;
        }

    }
}
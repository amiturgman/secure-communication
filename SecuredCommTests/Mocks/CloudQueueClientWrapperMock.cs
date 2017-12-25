using SecuredCommunication;

namespace UnitTests.Mocks
{
    public class CloudQueueClientWrapperMock : ICloudQueueClientWrapper
    {
        public CloudQueueWrapperMock cloudQueueWrapperMock;

        public ICloudQueueWrapper GetQueueReference(string queueName)
        {
            return cloudQueueWrapperMock ?? (cloudQueueWrapperMock = new CloudQueueWrapperMock());
        }
    }
}

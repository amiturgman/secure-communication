using System;
using SecuredCommunication;

namespace UnitTests.Mocks
{
    public class CloudQueueClientWrapperMock : ICloudQueueClientWrapper
    {
        public CloudQueueWrapperMock cloudQueueWrapperMock;

        public ICloudQueueWrapper GetQueueReference(string queueName)
        {
            if (cloudQueueWrapperMock == null)
            {
                cloudQueueWrapperMock = new CloudQueueWrapperMock();
            }

            return cloudQueueWrapperMock;
        }
    }
}

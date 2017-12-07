using System;
using Contracts;
using Xunit;
using SecuredCommunication;

namespace SecuredCommTests
{
    public class SecuredCommTests
    {
        [Fact]
        public void Sanity_VerifyCanBeCreated()
        {
            var secretsMock = (ISecretsManagement)new SecretsManagementMock();
            var sec = new RabbitMQBusImpl(secretsMock, false);

        }
    }
}

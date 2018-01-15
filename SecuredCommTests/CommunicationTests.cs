using Cryptography;
using Xunit;
using System.Configuration;

namespace UnitTests
{
    public class CommunicationTests
    {
        public CommunicationTests() {
            ConfigurationManager.AppSettings["rabbitMqUri"] = "amqp://localhost:5672";
            ConfigurationManager.AppSettings["AzureStorageConnectionString"] = "UseDevelopmentStorage=true";
        }

        /// <summary>
        /// Sanities the RabbitMQBusImpl can be created.
        /// </summary>
        [Fact]
        public void Sanity_VerifyRabbitMQBusImplCanBeCreated()
        {
            // todo:
            // var secretsMock = (ISecretsManagement)new SecretsManagementMock();
            // var sec = new RabbitMQBusImpl(secretsMock, false, "securedCommExchange");
        }

        /// <summary>
        /// Sanities the AzureQueueImpl can be created.
        /// </summary>
        [Fact]
        public void Sanity_VerifyAzureQueueImplCanBeCreated()
        {
            var secretsMock = (IEncryption)new EncryptionManagerMock();
            //var sec = new AzureQueueImpl("connectionstring", secretsMock, false);

        }
    }
}

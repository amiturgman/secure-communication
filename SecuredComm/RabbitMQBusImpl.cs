using System;
using System.Threading.Tasks;
using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecuredCommunication
{
    // An implementation using the RabbitMQ service
    public class RabbitMQBusImpl : IQueueManager
    {
        #region private members

        private readonly IEncryptionManager m_secretMgmt;
        private IModel m_channel;
        private readonly bool m_isEncrypted;
        private readonly string m_exchangeName;
        private EventingBasicConsumer m_consumer;
        private bool m_isInitialized;
        private string m_rabitMqUri;
        private IBasicProperties m_queueProperties;
        private string m_queueName;
        #endregion

        public RabbitMQBusImpl(
            string rabitMqUri,
            IEncryptionManager secretMgmnt,
            bool isEncrypted,
            string exchangeName,
            string queueName)
        {
            m_exchangeName = exchangeName;
            m_rabitMqUri = rabitMqUri;
            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
            m_queueName = queueName;
        }

        public void Initialize()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri(m_rabitMqUri)
            };

            IConnection conn = factory.CreateConnection();
            m_channel = conn.CreateModel();
            m_channel.ExchangeDeclare(m_exchangeName, ExchangeType.Direct);

            CreateQueue(m_queueName);

            m_queueProperties = m_channel.CreateBasicProperties();
            m_queueProperties.Persistent = true;
            m_channel.BasicQos(0, 1, false);

            m_isInitialized = true;
        }

        public Task<string> DequeueAsync(Action<byte[]> cb)
        {
            ThrowIfNotInitialized();

            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += (ch, ea) =>
            {
                // Ack to the queue that we got the message
                // TODO: handle messages that failed
                m_channel.BasicAck(ea.DeliveryTag, false);

                MessageUtils.ProcessQueueMessage(ea.Body, m_secretMgmt, cb);
            };

            // return the consumer tag
            return Task.FromResult(m_channel.BasicConsume(m_queueName, false, m_consumer));
        }

        public Task DequeueAsync(Action<byte[]> cb, TimeSpan waitTime)
        {
            throw new Exception("Not supported for rabbitMQ");
        }

        public Task EnqueueAsync(string data)
        {
            ThrowIfNotInitialized();
           
            var msgAsBytes = MessageUtils.CreateMessageForQueue(data, m_secretMgmt, m_isEncrypted);
            m_channel.BasicPublish(
                exchange: m_exchangeName,
                routingKey: m_queueName,
                mandatory: false,
                basicProperties: m_queueProperties,
                body: msgAsBytes);

            return Task.FromResult(0);
        }

        public void CancelListeningOnQueue(string consumerTag)
        {
            ThrowIfNotInitialized();

            m_channel.BasicCancel(consumerTag);
        }

        private void CreateQueue(string queueName)
        {
            ThrowIfNotInitialized();

            m_channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            m_channel.QueueBind(queueName, m_exchangeName, queueName);
        }

        private void ThrowIfNotInitialized()
        {
            if (!m_isInitialized)
            {
                // todo: add correct exc
                throw new Exception("Not initialized");
            }
        }
    }
}

using System;
using System.Configuration;
using System.Threading.Tasks;
using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecuredCommunication
{
    // An implementation using the RabbitMQ service
    public class RabbitMQBusImpl : IQueueCommunication
    {
        #region private members

        private readonly IEncryptionManager m_secretMgmt;
        private readonly IModel m_channel;
        private readonly bool m_isEncrypted;
        private readonly string m_exchangeName;
        private EventingBasicConsumer m_consumer;

        #endregion

        public RabbitMQBusImpl(
            string rabitMqUri,
            IEncryptionManager secretMgmnt,
            bool isEncrypted,
            string exchangeName)
        {
            // todo: move to init method
            m_exchangeName = exchangeName;
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri(rabitMqUri)
            };
            IConnection conn = factory.CreateConnection();

            m_channel = conn.CreateModel();
            m_channel.ExchangeDeclare(m_exchangeName, ExchangeType.Direct);

            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
        }

        public Task<string> DequeueAsync(string queueName, Action<byte[]> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += (ch, ea) =>
            {
                // ack to the queue that we got the msg
                // TODO: handle messages that failed
                m_channel.BasicAck(ea.DeliveryTag, false);

                MessageUtils.ProcessQueueMessage(ea.Body, m_secretMgmt, cb);
            };

            // return the consumer tag
            return Task.FromResult(m_channel.BasicConsume(queueName, false, m_consumer));
        }

        public Task EnqueueAsync(string queueName, string data)
        {
            //todo: move out
            CreateQueue(queueName);

            var properties = m_channel.CreateBasicProperties();
            properties.Persistent = true;
            // todo: add doc here
            m_channel.BasicQos(0, 1, false);
            // todo until here

            var msgAsBytes = MessageUtils.CreateMessageForQueue(data, m_secretMgmt, m_isEncrypted);
            m_channel.BasicPublish(
                exchange: m_exchangeName,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: msgAsBytes);

            return Task.FromResult(0);
        }

        public void CancelListeningOnQueue(string consumerTag)
        {
            m_channel.BasicCancel(consumerTag);
        }

        private void CreateQueue(string queueName)
        {
            m_channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            m_channel.QueueBind(queueName, m_exchangeName, queueName);
        }
    }
}

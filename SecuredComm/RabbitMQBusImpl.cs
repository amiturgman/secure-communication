using System;
using System.Configuration;
using System.Threading.Tasks;
using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecuredCommunication
{
    public class RabbitMQBusImpl : ISecuredComm
    {
        private ISecretsManagement m_secretMgmt;
        private EventingBasicConsumer m_consumer;
        private IModel m_channel;

        private static string c_exchangeName = "securedCommExchange";
        private bool m_isEncrypted;

        public RabbitMQBusImpl(
            ISecretsManagement secretMgmnt,
            bool isEncrypted)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = new Uri(ConfigurationManager.AppSettings["rabbitMqUri"])
            };
            IConnection conn = factory.CreateConnection();

            m_channel = conn.CreateModel();
            // topic based...
            // m_channel.ExchangeDeclare(c_exchangeName, ExchangeType.Direct);

            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
        }

        public Task<string> Dequeue(string queueName, Action<Message> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += async (ch, ea) =>
            {
                // ack to the queue that we got the msg
                // TODO: handle messages that failed
                m_channel.BasicAck(ea.DeliveryTag, false);

                await Message.DecryptAndVerifyQueueMessage(ea.Body, m_secretMgmt, cb);
            };

            // return the consumer tag
            return Task.FromResult(m_channel.BasicConsume(queueName, false, m_consumer));
        }

        public async Task EnqueueAsync(string queueName, string data)
        {
            CreateQueue(queueName);

            var properties = m_channel.CreateBasicProperties();
            properties.Persistent = true;
            m_channel.BasicQos(0, 1, false);

            var msgAsBytes = await Message.CreateMessageForQueue(data, m_secretMgmt, m_isEncrypted);
            m_channel.BasicPublish(
                exchange: c_exchangeName,
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: msgAsBytes);
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

            m_channel.QueueBind(queueName, c_exchangeName, queueName);
        }
    }
}

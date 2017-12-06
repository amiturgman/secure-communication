using System;
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
            Uri queueUri,
            bool isEncrypted)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                Uri = queueUri
            };
            IConnection conn = factory.CreateConnection();

            m_channel = conn.CreateModel();
            // topic based...
            // m_channel.ExchangeDeclare(c_exchangeName, ExchangeType.Direct);

            m_secretMgmt = secretMgmnt;
            m_isEncrypted = isEncrypted;
        }

        public string Dequeue(string queueName, Action<Message> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body;

                var msg = Utils.FromByteArray<Message>(body);
                if (msg.IsEncrypted)
                {
                    msg.Data = await m_secretMgmt.Decrypt(msg.Data);
                }

                var verifyResult = await m_secretMgmt.VerifyAsync(msg.Data, msg.Signature);
                
                // ack to the queue that we got the msg
                // TODO: handle messages that failed
                m_channel.BasicAck(ea.DeliveryTag, false);

                if (verifyResult == false)
                {
                    throw new Exception("Verify failed!!");
                }

                cb(msg);
            };

            // return the consumer tag
            return m_channel.BasicConsume(queueName, false, m_consumer);
        }

        public async Task EnqueueAsync(string queue, string data)
        {
            CreateQueue(queue);

            var properties = m_channel.CreateBasicProperties();
            properties.Persistent = true;
            m_channel.BasicQos(0, 1, false);

            var dataInBytes = Utils.ToByteArray(data);
            var msg = new Message();
            msg.IsSigned = true;
            msg.Signature = await m_secretMgmt.SignAsync(dataInBytes);
            msg.IsEncrypted = m_isEncrypted;

            if (m_isEncrypted)
            {
                var encMsg = await m_secretMgmt.Encrypt(dataInBytes);
                msg.Data = encMsg;
            }
            else
            {
                msg.Data = dataInBytes;
            }

            var msgAsBytes = Utils.ToByteArray(msg);
            m_channel.BasicPublish(
                exchange: c_exchangeName,
                routingKey: queue,
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

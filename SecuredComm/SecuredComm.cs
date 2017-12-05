using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecuredCommunication
{
    public class RabbitMQBusImpl: ISecuredComm
    {
        private ISecretsManagement m_secretMgmt;
        private EventingBasicConsumer m_consumer;
        private IModel m_channel;

        private static string c_exchangeName = "securedCommExchange";
        private string m_encryptionKeyName;
        private string m_decryptionKeyName;
        private string m_verificationKeyName;
        private string m_signKeyName;
        private bool m_isEncrypted;

        public RabbitMQBusImpl(
            ISecretsManagement secretMgmnt,
            Uri queueUri,
            string verificationKeyName,
            string signKeyName,
            bool isEncrypted)
            : this(secretMgmnt, queueUri, verificationKeyName, signKeyName, isEncrypted, string.Empty, string.Empty)
        {
        }

        public RabbitMQBusImpl(
            ISecretsManagement secretMgmnt,
            Uri queueUri,
            string verificationKeyName,
            string signKeyName,
            bool isEncrypted,
            string encryptionKeyName,
            string decryptionKeyName)
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
            m_encryptionKeyName = encryptionKeyName;
            m_decryptionKeyName = decryptionKeyName;
            m_verificationKeyName = verificationKeyName;
            m_signKeyName = signKeyName;
            m_isEncrypted = isEncrypted;
        }

        public string Dequeue(string queueName, Action<Message> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body;

                var msg = FromByteArray<Message>(body);
                if (msg.isEncrypted)
                {
                    msg.data = await m_secretMgmt.Decrypt(msg.data);
                }

                var verifyResult = await m_secretMgmt.Verify(msg.sign, msg.data);
                if (verifyResult == false)
                {
                    //throw;
                }

                // ack to the queue that we got the msg
                m_channel.BasicAck(ea.DeliveryTag, false);

                cb(msg);
            };

            // return the consumer tag
            return m_channel.BasicConsume(queueName, false, m_consumer);
        }

        public void CancelListeningOnQueue(string consumerTag)
        {
            m_channel.BasicCancel(consumerTag);
        }

        public async Task EnqueueAsync(string queue, Message msg)
        {
            CreateQueue(queue);

            var properties = m_channel.CreateBasicProperties();
            properties.Persistent = true;
            m_channel.BasicQos(0, 1, false);

            msg.isSigned = true;
            msg.sign = await m_secretMgmt.Sign(msg.data);
            msg.isEncrypted = m_isEncrypted;

            if (m_isEncrypted)
            {
                var encMsg = await m_secretMgmt.Encrypt(msg.data);
                msg.data = encMsg;
            }

            var msgAsBytes = ToByteArray(msg);
            m_channel.BasicPublish(
                exchange: c_exchangeName,
                routingKey: queue,
                mandatory: false,
                basicProperties: properties,
                body: msgAsBytes);
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

        #region private methods

        private byte[] ToByteArray<T>(T source)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        private T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }

        #endregion
    }
}

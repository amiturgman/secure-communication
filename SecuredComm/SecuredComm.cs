using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

// SB - if needed
//using Microsoft.ServiceBus.Messaging;

// Rabbit MQ
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecuredCommunication
{
    public class SecuredComm : ISecuredComm
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

        public SecuredComm(
            ISecretsManagement secretMgmnt,
            Uri queueUri, 
            string verificationKeyName,
            string signKeyName,
            bool isEncrypted) 
            : this(secretMgmnt, queueUri, verificationKeyName, signKeyName, isEncrypted, string.Empty, string.Empty)
        {
        }

        public SecuredComm(
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
            m_channel.ExchangeDeclare(c_exchangeName, ExchangeType.Topic);

            m_secretMgmt = secretMgmnt;
            m_encryptionKeyName = encryptionKeyName;
            m_decryptionKeyName = decryptionKeyName;
            m_verificationKeyName = verificationKeyName;
            m_signKeyName = signKeyName;
            m_isEncrypted = isEncrypted;
        }

        public string ListenOnQueue(string queueName, string[] topics, Action<Message> cb)
        {
            foreach (var topic in topics)
            {
                m_channel.QueueBind(queue: queueName,
                                    exchange: c_exchangeName,
                                    routingKey: topic);
            }
            
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body;

                var msg = FromByteArray<Message>(body);
                if (msg.isEncrypted) {
                    msg.data = await m_secretMgmt.Decrypt(msg.data);
                }

                var verifyResult = await m_secretMgmt.Verify(msg.sign, msg.data);
                if (verifyResult == false) {
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

        public async void SendMsgAsync(string topic, Message msg)
        {
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
                routingKey: topic,
                mandatory: false,
                basicProperties: null,
                body: msgAsBytes);
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

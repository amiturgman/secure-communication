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
        private const string GlobalKeyVaultName = "global";
        private const string PrivateKeyVaultName = "private";

        private static string c_exchangeName = "securedCommExchange";

        public SecuredComm(ISecretsManagement secretMgmnt, Uri queueUri)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = queueUri; 
            IConnection conn = factory.CreateConnection();

            m_channel = conn.CreateModel();
            // topic based...
            m_channel.ExchangeDeclare(c_exchangeName, ExchangeType.Topic);

            m_secretMgmt = secretMgmnt;
        }

        public string ListenOnQueue(string queueName, string[] topics, string verificationKeyName, Action<Message> cb, string decryptionKeyName = "")
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
                if (decryptionKeyName != string.Empty) {
                    msg.data = 
                        await m_secretMgmt.Decrypt(PrivateKeyVaultName,decryptionKeyName, msg.data);
                }

                var verifyResult = 
                    await m_secretMgmt.Verify(PrivateKeyVaultName, verificationKeyName, msg.sign, msg.data);
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

        public async Task SendEncryptedMsgAsync(string encKeyName, string signingKeyName, string queue, string topic, Message msg)
        {
            var encMsg = await m_secretMgmt.Encrypt(GlobalKeyVaultName, encKeyName, msg.data);
            msg.data = encMsg;
            msg.sign = await m_secretMgmt.Sign(GlobalKeyVaultName, signingKeyName, msg.data);

            msg.isEncrypted = true;
            msg.isSigned = true;
            await SendMsg(queue, topic, msg);
        }

        public async Task SendUnencryptedMsgAsync(string signingKeyName, string queue, string topic, Message msg)
        {
            msg.isEncrypted = false;

            // sign even if the msg is encrypted
            msg.sign = await m_secretMgmt.Sign(GlobalKeyVaultName, signingKeyName, msg.data);
            msg.isSigned = true;

            await SendMsg(queue, topic, msg);
        }

        #region private methods

        private async Task SendMsg(string queue, string topic, Message msg)
        {
            var msgAsBytes = ToByteArray<Message>(msg);
            m_channel.BasicPublish(
                exchange: c_exchangeName,
                routingKey: topic,
                mandatory: false,
                basicProperties: null,
                body: msgAsBytes);
            await Task.FromResult<object>(null);
        }

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

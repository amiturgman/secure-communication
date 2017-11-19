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

        public SecuredComm(ISecretsManagement secretMgmnt, Uri queueUri)
        {
            // todo: replace hard coded values with supplied uri 
            //factory.Uri = queueUri; //"amqp://user:pass@hostName:port/vhost";

            ConnectionFactory factory = new ConnectionFactory
            {
                UserName = "XXX",
                Password = "XXX",
                VirtualHost = "/",
                Protocol = Protocols.AMQP_0_9_1,//DefaultProtocol;//FromEnvironment();
                HostName = "1.1.1.1",
                Port = 5672
            };

            IConnection conn = factory.CreateConnection();

            m_channel = conn.CreateModel();
            m_channel.ExchangeDeclare("exchangeName", ExchangeType.Direct);

            m_secretMgmt = secretMgmnt;
        }

        public string ListenOnUnencryptedQueue(string verificationKeyName, string queueName, Action<Message> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body;
                var msg = FromByteArray<Message>(body);

                var verifyResult = await m_secretMgmt.Verify(verificationKeyName, msg.sign);
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

        public string ListenOnEncryptedQueue(string decryptionKeyName, string verificationKeyName, string queueName, Action<Message> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body;

                var msg = FromByteArray<Message>(body);
                msg.data = await m_secretMgmt.Decrypt(decryptionKeyName, msg.data);

                var verifyResult = await m_secretMgmt.Verify(verificationKeyName, msg.sign);
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

        public async Task SendEncryptedMsgAsync(string encKeyName, string signingKeyName, string queue, Message msg)
        {
            var encMsg = await m_secretMgmt.Encrypt(encKeyName, msg.data);
            msg.data = encMsg;
            msg.isEncrypted = true;
            msg.sign = await m_secretMgmt.Sign(signingKeyName, msg.data);
            await SendMsg(queue, msg);
        }

        public async Task SendUnencryptedMsgAsync(string signingKeyName, string queue, Message msg)
        {
            msg.isEncrypted = false;

            // sign even if the msg is encrypted
            msg.sign = await m_secretMgmt.Sign(signingKeyName, msg.data);
            await SendMsg(queue, msg);
        }

        #region private methods

        private async Task SendMsg(string queue, Message msg)
        {
            m_channel.QueueDeclare(queue, false, false, false, null);
            m_channel.QueueBind(queue, "exchangeName", queue, null);

            var msgAsBytes = ToByteArray<Message>(msg);
            m_channel.BasicPublish(
                exchange: "",
                routingKey: queue,
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

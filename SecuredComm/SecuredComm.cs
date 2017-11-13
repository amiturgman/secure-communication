using System;
using System.Threading.Tasks;

// SB - if needed
//using Microsoft.ServiceBus.Messaging;

// Rabbit MQ
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SecuredComm
{
    public class SecuredComm : ISecuredComm
    {
        private ISecretsManagement m_secretMgmt;
        private EventingBasicConsumer m_consumer;
        private IModel m_channel;

        public SecuredComm(ISecretsManagement secretMgmnt, Uri queueUri) {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = queueUri; //"amqp://user:pass@hostName:port/vhost";

            IConnection conn = factory.CreateConnection();
            m_channel = conn.CreateModel();

            m_secretMgmt = secretMgmnt;
        }

        public string ListenOnUnencryptedQueue(string queueName, Action<string> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                m_channel.BasicAck(ea.DeliveryTag, false);
                cb("msg"); //body);
            };

            // return the consumer tag
            return m_channel.BasicConsume(queueName, false, m_consumer);
        }

        public string ListenOnEncryptedQueue(string decryptionKeyName, string queueName, Action<string> cb)
        {
            m_consumer = new EventingBasicConsumer(m_channel);
            m_consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                var decryptedMsg = "";// SecretManagement.Decrypt(decryptionKeyName, body);
                m_channel.BasicAck(ea.DeliveryTag, false);
                cb(decryptedMsg);
            };

            // return the consumer tag
            return m_channel.BasicConsume(queueName, false, m_consumer);
        }

        public void CancelListeningOnQueue(string consumerTag) {
            m_channel.BasicCancel(consumerTag);
        }

        async Task ISecuredComm.SendEncryptedMsgAsync(string encKeyName, string queue, string msg)
        {
            // var encMsg = secretMgmt.EncryptMsg(encKeyName, msg);
            var encMsg = "";
            await SendMsg(encMsg);
        }

        async Task ISecuredComm.SendUnencryptedMsgAsync(string queue, string msg)
        {
            await SendMsg(msg);
        }

        private async Task SendMsg(string msg){
            await Task.FromResult<object>(null);
        }
    }
}

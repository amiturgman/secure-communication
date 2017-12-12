using System;
using System.Threading.Tasks;

namespace Contracts
{
    [Serializable]
    public class Message
    {
        public bool IsEncrypted;
        public bool IsSigned;
        public byte[] Data;
        public byte[] Signature;

        public static async Task<byte[]> CreateMessageForQueue(string data, ISecretsManagement secretsManagement, bool isEncrypted)
        {
            var dataInBytes = Utils.ToByteArray(data);
            var msg = new Message();
            msg.IsSigned = true;
            //msg.Signature = await secretsManagement.SignAsync(dataInBytes);
            msg.IsEncrypted = isEncrypted;

            if (isEncrypted)
            {
                var encMsg = await secretsManagement.Encrypt(dataInBytes);
                msg.Data = encMsg;
            }
            else
            {
                msg.Data = dataInBytes;
            }

            return Utils.ToByteArray(msg);
        }

        public static async Task DecryptAndVerifyQueueMessage(byte[] body, ISecretsManagement secretsManagement, Action<Message> cb)
        {
            var msg = Utils.FromByteArray<Message>(body);
            if (msg.IsEncrypted)
            {
                msg.Data = await secretsManagement.Decrypt(msg.Data);
            }

            //var verifyResult = await secretsManagement.VerifyAsync(msg.Data, msg.Signature);

            //if (verifyResult == false)
            //{
            //    throw new Exception("Verify failed!!");
            //}

            cb(msg);
        }
    }

    public interface ISecuredComm
    {
        /// <summary>
        /// Enqueue a message to the queue
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <param name="msg">Message.</param>
        Task EnqueueAsync(string queueName, string msg);

        /// <summary>
        /// Creates a listener on a queue where messages are encrypted. The message's data is automatically decrypted
        /// </summary>
        /// <returns>The consumer tag</returns>
        Task<string> Dequeue(string queueName, Action<Message> cb);
    }
}
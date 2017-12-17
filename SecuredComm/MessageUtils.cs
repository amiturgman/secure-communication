using System;
using System.Threading.Tasks;
using Contracts;

namespace SecuredCommunication
{
    /// <summary>
    /// Helper utility methods related to the <see cref="Message"/> class
    /// </summary>
    public static class MessageUtils
    {
        public static async Task<byte[]> CreateMessageForQueue(string data, IEncryptionManager encryptionManager, bool isEncrypted)
        {
            var dataInBytes = Utils.ToByteArray(data);
            var signature = await encryptionManager.SignAsync(dataInBytes);

            if (isEncrypted)
            {
                dataInBytes = await encryptionManager.Encrypt(dataInBytes);
            }

            return Utils.ToByteArray(new Message(isEncrypted, dataInBytes, signature));
        }

        public static async Task DecryptAndVerifyQueueMessage(byte[] body, IEncryptionManager encryptionManager, Action<Message> cb)
        {
            var msg = Utils.FromByteArray<Message>(body);
            if (msg.m_isEncrypted)
            {
                msg.m_data = await encryptionManager.Decrypt(msg.m_data);
            }

            var verifyResult = await encryptionManager.VerifyAsync(msg.m_data, msg.m_signature);

            if (verifyResult == false)
            {
                throw new Exception("Verify failed!!");
            }

            cb(msg);
        }
    }
}

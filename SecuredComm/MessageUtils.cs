using System;
using Contracts;

namespace SecuredCommunication
{
    /// <summary>
    /// Helper utility methods related to the <see cref="Message"/> class
    /// </summary>
    public static class MessageUtils
    {
        public static byte[] CreateMessageForQueue(string data, IEncryptionManager encryptionManager, bool isEncrypted)
        {
            var dataInBytes = Utils.ToByteArray(data);
            var signature = encryptionManager.Sign(dataInBytes);

            if (isEncrypted)
            {
                dataInBytes = encryptionManager.Encrypt(dataInBytes);
            }

            return Utils.ToByteArray(new Message(isEncrypted, dataInBytes, signature));
        }

        /// <summary>
        /// Decrypts (if encrypted), Verifies and runs the callback on the recieved queue message
        /// </summary>
        /// <param name="body">Body.</param>
        /// <param name="encryptionManager">Encryption manager.</param>
        /// <param name="cb">Cb.</param>
        public static void ProcessQueueMessage(byte[] body, IEncryptionManager encryptionManager, Action<byte[]> cb)
        {
            var msg = Utils.FromByteArray<Message>(body);
            var data = msg.Data;
            if (msg.Encrypted)
            {
                data = encryptionManager.Decrypt(msg.Data);
            }

            var verifyResult = encryptionManager.Verify(data, msg.Signature);

            if (verifyResult == false)
            {
                throw new Exception("Verify failed!!");
            }

            cb(data);
        }
    }
}

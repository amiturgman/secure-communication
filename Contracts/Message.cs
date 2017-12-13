using System;
using System.Threading.Tasks;

namespace Contracts
{
    [Serializable]
    public class Message
    {
        public bool IsEncrypted;
        public byte[] Data;
        public byte[] Signature;

        public Message(bool isEncrypted, byte[] data, byte[] signature)
        {
            IsEncrypted = isEncrypted;
            Data = data;
            Signature = signature;
        }

        public static async Task<byte[]> CreateMessageForQueue(string data, IEncryptionManager encryptionManager, bool isEncrypted)
        {
            var dataInBytes = Utils.ToByteArray(data);
            var signature = await encryptionManager.SignAsync(dataInBytes);

            if (isEncrypted)
            {
                dataInBytes = await encryptionManager.Encrypt(dataInBytes);
            }

            var msg = new Message(isEncrypted, dataInBytes, signature);
            return Utils.ToByteArray(msg);
        }

        public static async Task DecryptAndVerifyQueueMessage(byte[] body, IEncryptionManager encryptionManager, Action<Message> cb)
        {
            var msg = Utils.FromByteArray<Message>(body);
            if (msg.IsEncrypted)
            {
                msg.Data = await encryptionManager.Decrypt(msg.Data);
            }

            var verifyResult = await encryptionManager.VerifyAsync(msg.Data, msg.Signature);

            if (verifyResult == false)
            {
                throw new Exception("Verify failed!!");
            }

            cb(msg);
        }
    }}

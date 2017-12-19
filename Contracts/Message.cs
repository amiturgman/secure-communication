using System;

namespace Contracts
{
    /// <summary>
    /// A message object which is passed on the communication pipeline.
    /// </summary>
    [Serializable]
    public class Message
    {
        public bool Encrypted { get; }
        public byte[] Data { get; }
        public byte[] Signature { get; }

        public Message(bool isEncrypted, byte[] data, byte[] signature)
        {
            if (data == null || signature == null)
            {
                throw new ArgumentException("invalid input data");
            }

            Encrypted = isEncrypted;
            Data = data;
            Signature = signature;
        }
    }
}

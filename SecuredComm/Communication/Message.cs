using System;

namespace Communication
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

        /// <summary>
        /// Ctor for message that is passed in the communication pipeline
        /// </summary>
        /// <param name="isEncrypted">A flag indicates whether the message is Encrypted</param>
        /// <param name="data">A byte array of the data to send</param>
        /// <param name="signature">The signature on the data</param>
        public Message(bool isEncrypted, byte[] data, byte[] signature)
        {
            Encrypted = isEncrypted;
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Signature = signature ?? throw new ArgumentNullException(nameof(signature));
        }
    }
}

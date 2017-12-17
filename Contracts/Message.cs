using System;

namespace Contracts
{
    /// <summary>
    /// A message object which is passed on the communication pipeline.
    /// </summary>
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
    }}

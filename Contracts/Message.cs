using System;

namespace Contracts
{
    /// <summary>
    /// A message object which is passed on the communication pipeline.
    /// </summary>
    [Serializable]
    public class Message
    {
        public bool m_isEncrypted;
        public byte[] m_data;
        public byte[] m_signature;

        public Message(bool isEncrypted, byte[] data, byte[] signature)
        {
            m_isEncrypted = isEncrypted;
            m_data = data;
            m_signature = signature;
        }
    }}

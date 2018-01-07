using System;
using System.Runtime.Serialization;

namespace SecuredCommunication
{
    /// <summary>
    /// This class will wrap all secure communication handled exceptions
    /// </summary>
    public class SecureCommunicationException : Exception
    {
        public SecureCommunicationException()
        {
        }

        public SecureCommunicationException(string message) : base(message)
        {
        }

        public SecureCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SecureCommunicationException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {

        }
    }

    public class SignatureVerificationException : SecureCommunicationException
    {
        public SignatureVerificationException()
        {
        }

        public SignatureVerificationException(string message) : base(message)
        {
        }

        public SignatureVerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SignatureVerificationException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
        }
    }

    public class DecryptionException : SecureCommunicationException
    {
        public DecryptionException()
        {
        }

        public DecryptionException(string message) : base(message)
        {
        }

        public DecryptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DecryptionException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
        }
    }

    public class EncryptionException : SecureCommunicationException
    {
        public EncryptionException()
        {
        }

        public EncryptionException(string message) : base(message)
        {
        }

        public EncryptionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EncryptionException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
        }
    }
}
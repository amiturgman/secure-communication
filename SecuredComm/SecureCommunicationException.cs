using System;

namespace SecuredComm
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
    }

    public class SignatureVerificationException : SecureCommunicationException
    {
        public SignatureVerificationException(string message)
            : base(message) { }
    }

    public class DecryptionException : SecureCommunicationException
    {
        public DecryptionException(string message)
            : base(message) { }

        public DecryptionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
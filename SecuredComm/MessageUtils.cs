using System;
using System.Security.Cryptography;
using Contracts;
using SecuredComm;
using DecryptionException = SecuredComm.DecryptionException;

namespace SecuredCommunication
{
    /// <summary>
    /// Helper utility methods related to the <see cref="Message"/> class
    /// </summary>
    public static class MessageUtils
    {
        /// <summary>
        /// Encrypts (if needed), signs and converts the message to byte array
        /// </summary>
        /// <param name="data">The data to send on the queue</param>
        /// <param name="encryptionManager">The encryption manager</param>
        /// <param name="isEncrypted">A flag that indicates whether the message needs to be encrypted</param>
        /// <returns>A byte array representing the message</returns>
        public static byte[] CreateMessageForQueue(string data, IEncryptionManager encryptionManager, bool isEncrypted)
        {
            if (encryptionManager == null)
            {
                throw new ArgumentNullException(nameof(encryptionManager));
            }

            // Convert the data to byte array
            var dataInBytes = data != null ? Utils.ToByteArray(data) : throw new ArgumentNullException("data");

            // Sign the message
            var signature = encryptionManager.Sign(dataInBytes);

            if (isEncrypted)
            {
                try
                {
                    // Encrypt the message
                    dataInBytes = encryptionManager.Encrypt(dataInBytes);
                }
                catch (CryptographicException ex)
                {
                    throw new EncryptionException($"Encryption failed: {ex.Message}");
                }
            }

            // Convert the message to byte array
            return Utils.ToByteArray(new Message(isEncrypted, dataInBytes, signature));
        }

        /// <summary>
        /// Decrypts (if encrypted), Verifies and runs the callback on the received queue message
        /// </summary>
        /// <param name="messageInBytes">The message in bytes.</param>
        /// <param name="encryptionManager">Encryption manager.</param>
        /// <param name="callback">The callback to preform once the message is decrypted (if needed) and verified</param>
        public static void ProcessQueueMessage(byte[] messageInBytes, IEncryptionManager encryptionManager,
            Action<byte[]> callback)
        {
            // Deserialize the  byte array to Message object
            var msg = messageInBytes != null
                ? Utils.FromByteArray<Message>(messageInBytes)
                : throw new ArgumentNullException("messageInBytes");

            var data = msg.Data;
             
            if (msg.Encrypted)
            {
                try
                {
                    // Decrypt the message
                    data = encryptionManager.Decrypt(msg.Data);
                }
                catch (CryptographicException ex)
                {
                    throw new DecryptionException("Decryption failed", ex);
                }

            }

            // Verify the signature
            var verifyResult = encryptionManager.Verify(data, msg.Signature);

            if (verifyResult == false)
            {
                throw new SignatureVerificationException("Verify queue message failed. Check if the verification is" +
                                                         "done with the correct key");
            }

            // Call callback
            callback(data);
        }
    }
}

using System;
namespace SecuredCommunication
{
    public class TXEngine
    {
        ISecuredComm m_securedComm;

        // an example of a TXengine which uses the securedComm class
        public TXEngine()
        {
            /// ######## Usage Examples ##############
            var secretsMgmnt = new SecretsManagement("kvName", "appId", "servicePrincipalId"); // make it support multiple kvs
            m_securedComm = new SecuredComm(secretsMgmnt, new Uri("queueUri")); // add signing key/authority

            // can listen on encrypted queue, automatically decrypt messages
            m_securedComm.ListenOnEncryptedQueue(
                "SomeDecryptionKeyName1",
                "someQueue1",
                (decryptedMsg) =>
                {
                    Console.WriteLine("The decrypted msg is " + decryptedMsg.data);
                });

            // and listen on other queues...
            m_securedComm.ListenOnEncryptedQueue(
                "SomeDecryptionKeyName2",
                "someQueue2",
                (decryptedMsg) =>
                {
                    Console.WriteLine("The decrypted msg is " + decryptedMsg.data);
                });

            // even if unencrypted
            m_securedComm.ListenOnUnencryptedQueue(
                "someQueue3",
                (plainTextMsg) =>
                {
                    Console.WriteLine("The msg is " + plainTextMsg.data);
                });
        }

        public async void SendTX(string fromAddres, string toAddress, string value)
        {
            // Create the transaction
            var txMsg = "";
            Message msg = new Message(txMsg);
            await m_securedComm.SendEncryptedMsgAsync("EncryptionKeyName", "queueName", msg);
        }

        // example of unencrypted msg
        public async void SendNotification()
        {
            Message msg = new Message("notification of some kind");

            // Sends an unecnrypted msg
            await m_securedComm.SendUnencryptedMsgAsync("queueName", msg);
        }
    }
}

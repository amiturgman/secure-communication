using System;
using static SecuredCommunication.SecretsManagement;

namespace SecuredCommunication
{
    public class TXEngine
    {
        ISecuredComm m_securedComm;

        // an example of a TXengine which uses the securedComm class
        public TXEngine()
        {
            /// ######## Usage Examples ##############
            var kvInfo = new KeyVaultInfo("kvName", "appId", "servicePrincipalId");
            var secretsMgmnt = new SecretsManagement(kvInfo);
            m_securedComm = new SecuredComm(secretsMgmnt, new Uri("queueUri")); // add signing key/authority

            var topicsList = new string[1] { "topics1" };
            // can listen on encrypted queue, automatically decrypt messages
            m_securedComm.ListenOnQueue(
                "SomeDecryptionKeyName1",
               /// "SomeVerificationKeyName1",
                "someQueue1",
                topicsList,
                // todo? add here the list of auth senders
                (decryptedMsg) =>
                {
                    Console.WriteLine("The decrypted msg is " + decryptedMsg.data);
                });

            // and listen on other queues...
            m_securedComm.ListenOnQueue(
                "SomeDecryptionKeyName2",
           //     "SomeVerificationKeyName2",
                "someQueue2",
                topicsList,
                (decryptedMsg) =>
                {
                    Console.WriteLine("The decrypted msg is " + decryptedMsg.data);
                });

            // even if unencrypted
            m_securedComm.ListenOnQueue(
                "SomeVerificationKeyName3",
                "someQueue3",
                topicsList,
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
            await m_securedComm.SendEncryptedMsgAsync("EncryptionKeyName", "signingKeyName", "queueName", "topic1", msg);
        }

        // example of unencrypted msg
        public async void SendNotification()
        {
            Message msg = new Message("notification of some kind");

            // Sends an unecnrypted msg
            await m_securedComm.SendUnencryptedMsgAsync("queueName", "signingKeyName", "topic1", msg);
        }
    }
}

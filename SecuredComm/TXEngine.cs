using System;
namespace SecuredComm
{
    public class TXEngine
    {
        ISecuredComm m_securedComm;

        // an example of a TXengine which uses the securedComm class
        public TXEngine()
        {
            var secretsMgmnt = new SecretsManagement("kvName", "appId", "servicePrincipalId");
            m_securedComm = new SecuredComm(secretsMgmnt, new Uri("queueUri"));

            // can listen on encrypted queue, automatically decrypt messages
            m_securedComm.ListenOnEncryptedQueue(
                "SomeDecryptionKeyName1",
                "someQueue1",
                (decryptedMsg) =>
                {
                    Console.WriteLine("The decrypted msg is " + decryptedMsg);
                });

            // and listen on other queues...
            m_securedComm.ListenOnEncryptedQueue(
                "SomeDecryptionKeyName2",
                "someQueue2",
                (decryptedMsg) =>
                {
                    Console.WriteLine("The decrypted msg is " + decryptedMsg);
                });

            // even if unencrypted
            m_securedComm.ListenOnUnencryptedQueue(
                "someQueue3",
                (plainTextMsg) =>
                {
                    Console.WriteLine("The msg is " + plainTextMsg);
                });
        }

        public async void SendTX(string fromAddres, string toAddress, string value) {
            // Create the transaction
            // pseudo code
            // var msg = create tx
            var txMsg = "";
            await m_securedComm.SendEncryptedMsgAsync("EncryptionKeyName", "queueName", txMsg);
        }

        // example of unencrypted msg
        public async void SendNotification()
        {
            // Sends an unecnrypted msg

            // pseudo
            // var msg = create msg
            var msg = "";
            await m_securedComm.SendUnencryptedMsgAsync("queueName", msg);
        }
    }
}

# Core modules for crypto-currency virtual wallet
The project consists of the infrastructure Core modules needed for implementing a SaaS cryptocurrency virtual wallet. This project has the following modules:
### A secure communication library over a queue
For inter micro-services communication
### An Ethereum node client
For querying, signing and sending transactions and data over the public (and test) Ethereum network
### Secrets manager for the communication pipeline
For abstracting the needed secrets for the encryption/signing operations over the sent messages

This project also contains a [Sample](Sample) directory, to get you started.  

# Installation
1. `Contracts` contains all of the interfaces
2. `SecuredComm` contains the library implementation. consume it: clone the repository and add the dependency to the library.
(later we might release this as a nuget / package).
3. Usage examples:

## Ethereum node wrapper
```c#
// Create the instance
var ethereumNodeWrapper = new EthereumNodeWrapper(kv, ConfigurationManager.AppSettings["EthereumNodeUrl"]);

// Call methods
var result = await ethereumNodeWrapper.GetPublicAddressAsync("0x012345...");   
```

## Secrets Manager
```c#
// Create
var secretsMgmnt = new KeyVaultSecretManager(encryptionKeyName, decryptionKeyName, signKeyName, verifyKeyName, publicKv, privateKv);
// Initialize
await secretsMgmnt.Initialize();

// Call methods
secretsMgmnt.Encrypt(msgAsBytes);  
```
## Communication pipeline
```c#
// The following code enqueues a message to a queue named 'MyQueue'
// Create
var comm = new AzureQueueImpl("MyQueue", queueClient, secretsMgmnt, true);
// Init
await comm.Initialize();

// Enqueue messages
comm.EnqueueAsync("Some message meant for someone");

comm.DequeueAsync(msg =>
  {
    Console.WriteLine("Decrypted and Verified message is" : + msg);
  });
  
```

# Sample
## Installation instructions

TODO: update instruction once the oneclick script is ready

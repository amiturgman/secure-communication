# Core modules for crypto-currency virtual wallet [![Build Status](https://travis-ci.org/Azure/Secured-SaaS-Wallet.png?branch=master)](https://travis-ci.org/Azure/Secured-SaaS-Wallet)
The project consists of the infrastructure Core modules needed for implementing a SaaS cryptocurrency virtual wallet. This project has the following modules:
### Secrets manager for the communication pipeline
For abstracting the needed secrets for the encryption/signing operations over the sent messages
### A secure communication library over a queue
For inter micro-services communication
### An Ethereum node client
For querying, signing and sending transactions and data over the public (and test) Ethereum network

This project also contains a [Sample](WalletSample) directory, to get you started.  

# Installation
The project contains three components:
1. `Blockchain` - Blockchain (Currently only Ethereum implementation) related functionality <br>
2. `Communication` - Communication pipeline between micro-services.<br>
3. `Cryptography` - Provides functionality for saving the users secrets (private keys) and for securing the micro-services communication pipeline <br>

To consume, clone the repository and add the projects as dependencies.

# Usage examples:

## Secrets Manager
```c#

// Create
var kv = new KeyVault(...);

var secretsMgmnt =
                new KeyVaultCryptoActions(
                    new CertificateInfo(encryptionKeyName, encryptionCertPassword),
                    new CertificateInfo(decryptionKeyName, decryptionCertPassword),
                    new CertificateInfo(signKeyName, signCertPassword),
                    new CertificateInfo(verifyKeyName, verifyCertPassword),
                    kv,
                    kv);

// Initialize
await secretsMgmnt.InitializeAsync();

// Call methods
var rawData = "Some text";
var encryptedData = secretsMgmnt.Encrypt(Communication.Utils.ToByteArray(rawData));
var originalData = secretsMgmnt.Decrypt(encryptedData);

```
## Communication pipeline
```c#
// The following code enqueues a message to a queue named 'MyQueue'
var secretsMgmnt = new KeyVaultCryptoActions(...);
secretsMgmnt.InitializeAsync().Wait();

var queueClient = new CloudQueueClientWrapper(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
// Create
var securedComm = new AzureQueue("MyQueue", queueClient, secretsMgmnt, true);
// Init
await securedComm.InitializeAsync();

// Enqueue messages
await securedComm.EnqueueAsync(Communication.Utils.ToByteArray("A message"));

 securedComm.DequeueAsync(
   msg =>
   {
      Console.WriteLine("Decrypted and Verified message is" : + msg);
   });
  
```

## Ethereum node client
```c#
// Create the instance of the Sql connector (which holds the users' private keys)
var sqlDb = new SqlConnector(...);
// Create the instance
var ethereumNodeClient = new EthereumAccount(sqlDb, ConfigurationManager.AppSettings["EthereumNodeUrl"]);

// Call methods
var result = await ethereumNodeClient.GetPublicAddressAsync("0x012345...");   
```

# Sample
## Installation instructions

The following instructions will help you get started with a working SaaS wallet deployed on Azure.

### Prerequisites
1. An Azure subscription
2. Setup an Azure Active Directory application
3. Prepare the [Service principal id and secret and assign it to a contributor role](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-create-service-principal-portal)
4. An Azure Active Directory TenantId - Same link as step 3
5. Prepare the Azure Service principal object id
   1. Go to the azure portal
   2. Go to Azure Active Directory -> Enterprise Applications -> All applications. 
   3. Filter for your recently created application. One of the columns is 'Object Id'. 
   4. Copy this value for later use.
7. Install [Azure Powershell SDK](https://www.microsoft.com/web/handlers/webpi.ashx/getinstaller/WindowsAzurePowershellGet.3f.3f.3fnew.appids)

### Deploy resources
1. Edit the parameters in the file [WalletSample/Deployment/oneclick.ps1](WalletSample/Deployment/oneclick.ps1)
2. Open the Powershell console **(As administrator)** and navigate to the file's location.
3. run .\oneclick.ps1
   1. The script will create a resource group and deploy different resources into it. Some of the resources are Azure Service Fabric cluster, storage accounts, KeyVaults, Azure SQL, etc...


The script will take a few minutes to finish.
Once done:
1) Go to certificates folder (*c:\saaswalletcertificates* if left the certificate folder location as is), a pfx file will be present.
Install it under 'Local Computer\Personal' (the password was specified in the script earlier)
2) Open up the solution in Visual Studio **(As administrator)**
3) Update [cloud.xml](WalletSF/publishprofiles/cloud.xml) with the Service Fabric cluster name and the certificate thumbprint (needed for deploying to application)
4) Update [appsettings.json](WalletApp/appsettings.json) and [App.config](TransactionGenerator/App.config) file with your Azure resources information
* The 'EthereumNodeUrl' paramater should be generated by creating a new public/test account:
https://infura.io/docs/gettingStarted/chooseaNetwork
5) Right click on the 'WalletService', click publish, choose the newly created Service Fabric cluster
6) Once done, navigate to \<SFClusterName\>.\<location\>.cloudapp.azure.com/

## Usage
1. Create a new account
![Creating a new account for identifier 'MSFT_BLOCKCHAIN'](/images/createAccount.png)
2. Seed the account with ethereum coins:
https://faucet.rinkeby.io/
![Seeding the 'MSFT_BLOCKCHAIN' account with a few coins](/images/seed.png)
3. Transfer funds to another (already created account)
![Tranfer funds to another address](/images/sendFunds.png)
4. Press the 'Get balance' button
![Get the current balance](/images/getBalance.png)

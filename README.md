# secure-communication
Secure Communication Library Over a Queue

# Sample
## Installation instructions
### Prerequisites
1. An Azure subscription
2. Create a new Azure Active Directory application. This application will be used for authenticating against Azure: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications
3. Get the service principal id and secret for the application from step 2

### The setup script
The setup script will create a new Azure Resource Group and a new Azure Storage and Azure KeyVault in the new resource group, finally it will upload a new certificate as a secret to the Key Vault.
Run the script:
1. Edit the parameters in the file Sample/Deployment/oneclick.ps1, choose a region, a name for the resource groups, azure storage and keyvault
2. In a powershell console, go to Sample/Deployment
3. run oneclick.ps1

### The sample apps
The sample contains 3 different processes. 
1. **Coins sender**: a user that sends Ethereum coins to the receiver account, this process writes a signed and encrypted message on the transaction queue with the transaction details (sender, receiver and amount).
2. **Transaction Engine**: Reads from the transaction queue, verifies the signature and decrypts the message.
Then it signs the transaction and send it the Ethereum node. Finally it writes a message to the notification queue, to notify that the transaction completed and the receiver's balance had changed.
3. **Coins receiver**: Listens on the notification queue and checks if there was a change in the receiver balance.

#### How to run Ethereum in the sample apps
- Option 1: Work with Ethereum testnet.
Create a token [here](https://infura.io/#how-to) and fill it in EthereumNodeUrl parameter in the App.config.
- Option 2: Work with local Ethereum node - TestRpc. 
The fastest way to run it is with Docker container, you should run it with the following command (it will automatically creats 2 accounts, one of them with 300 Ethereums):
```
docker run -d -p 8545:8545 trufflesuite/ganache-cli:latest --account="0x4faec59e004fd62384813d760e55d6df65537b4ccf62f268253ad7d4243a7193, 300000000000000000000" --account="0x03fd5782c37523be6598ca0e5d091756635d144e42d518bb5f8db11cf931b447, 0"
```
#### Running the sample apps
1. Edit App.Config for all 3 of the sample applications. Fill in the missing values.
2. Compile and run all 3 projects in the following order: coins sender, transaction engine, coins receiver.

# secure-communication
Secure Communication Library Over a Queue

# Sample
## Installation instructions
### Prerequisites
1. An Azure subscription
2. Create a new application. This application will be used for authenticating against Azure: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications
3. Get the service principal id and secret for the application from step 2

### Running the script
1. Edit the paramaters in the file Sample/Deployment/oneclick.ps1, choose a region, a name for the resource groups, azure storage and keyvault
2. In a powershell console, go to Sample/Deployment
3. run oneclick.ps1

### Running the sample app
1. Edit App.Config for all 3 of the sample applications. Fill in the missing values.
2. Compile and run all 3 projects

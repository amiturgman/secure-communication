# This script creates a resource group and deploys into it an Azure KeyVault, 
# AzureStorage, Service Fabric cluster and a SQL server/database.
# Next, it populates the KeyVault with a certificate stored as a secret 
# (pfx - which contains public and private key)

#### Run this script with Admin Privileges #####

# Parameters

# The Azure tenant id (under active directory properties -> directory Id)
$tenantId  = '[Azure tenant id]'
# The Azure service principal's object id
$objectId = '[Azure Service principal object id]'
# The Azure Service principal application id
$applicationId= '[Azure application id]'
# The  Azure Service principal secret
$applicationSecret = "[Azure application key]"
# The Azure resource group name that will hold all the resources
$resourceGroupName = '[The resource group name]'
# The Azure resources geo location
$resourcesLocation = '[The resources location]'

# The Azure KeyVault Name
$keyvaultName = '[The vault name]'
# The Azure KeyVault secret name
$secretName = '[The keyvault secret name example: encryptionCert]'
# Azure storage name to create - will hold the Azure Queue
$storageName = '[Azure storage name]'

#certificate configuration
# The temporary pfx location
$pfxFilePath = '[The pfx temporary file, example: c:\temp\certificate.pfx]'
# Temporary password, for installing and exporting the certificate
$plainpass = '123456' 
# The certificate DNS name
$dnsName = '[The certificate dns name, example: testcert.contoso.com]'

# Queue
$storageAccountName = '[storage account name]'
$queueName = '[azure queue name, the one inside the storage account]'

# Service Fabric
$sfClusterName = '[Service Fbric cluster name]'
$clustersize = 3
$adminuser = 'nimda13'
$adminpwd="Password#1234" | ConvertTo-SecureString -AsPlainText -Force 

$certpwd="Password#1234" | ConvertTo-SecureString -AsPlainText -Force
$certfolder="c:\saaswalletcertificates\"

$subname= "$sfClusterName.$resourcesLocation.cloudapp.azure.com"
$vmsku = "Standard_D2_v2"

# SCRIPT START

# Login to Azure with service principal
$SecurePassword = $applicationSecret | ConvertTo-SecureString -AsPlainText -Force
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $applicationId, $SecurePassword
Add-AzureRmAccount -Credential $cred -Tenant $tenantId -ServicePrincipal

# Create Azure Resource group
New-AzureRmResourceGroup -Name $resourceGroupName -Location $resourcesLocation

# Deploy SQL server and configure it
$script = $PSScriptRoot + "\deploySqlDB.ps1"
& $script -resourcegroupname $resourceGroupName -location $resourcesLocation -adminlogin 'nimda12' -password 'adminPass#12!word'

# make sure the path exists
New-Item -ItemType Directory -Force -Path $certfolder

# Create the Service Fabric cluster.
New-AzureRmServiceFabricCluster -Name $sfClusterName -ResourceGroupName $resourceGroupName -Location  "SouthCentralUS" `
-ClusterSize $clustersize -VmPassword $adminpwd -CertificateSubjectName $subname `
-CertificateOutputFolder $certfolder -CertificatePassword $certpwd `
-OS WindowsServer2016Datacenter

# Create Azure storage (queue)
$storageAccount = New-AzureRmStorageAccount -ResourceGroupName $resourceGroupName `
  -Name $storageAccountName `
  -Location $resourcesLocation `
  -SkuName Standard_LRS

$ctx = $storageAccount.Context
$queue = New-AzureStorageQueue -Name $queueName -Context $ctx

# Creates the certificate
$cert = New-SelfSignedCertificate -certstorelocation cert:\localmachine\my -dnsname $dnsName
$pwd = ConvertTo-SecureString -String $plainpass -Force -AsPlainText
$path = 'cert:\localMachine\my\' + $cert.thumbprint 
Export-PfxCertificate -cert $path -FilePath $pfxFilePath -Password $pwd

# Store the certificate in the AzureKeyVault
$flag = [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
$collection = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2Collection
$collection.Import($pfxFilePath, $plainpass, $flag)
$pkcs12ContentType = [System.Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12
$clearBytes = $collection.Export($pkcs12ContentType, $plainpass)
$fileContentEncoded = [System.Convert]::ToBase64String($clearBytes)
$secret = ConvertTo-SecureString -String $fileContentEncoded -Force -AsPlainText
$secretContentType = 'application/x-pkcs12'

# Create the Key vault
New-AzureRmKeyVault -VaultName $keyvaultName -ResourceGroupName $resourceGroupName -Location $resourcesLocation
Set-AzureRmKeyVaultAccessPolicy -VaultName $keyvaultName -ObjectId $objectId -PermissionsToSecrets Get,Set,List
Set-AzureKeyVaultSecret -VaultName $keyvaultName -Name $secretName -SecretValue $secret -ContentType $secretContentType

# Delete local Certificate 
Remove-Item -path $pfxFilePath

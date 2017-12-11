 This script creates a resource group and deploys into it an Azure KeyVault and AzureStorage.
# Next, it populates the KeyVault with a certificate stored as a secret (pfx - which contains public and private key)

# parameters
$resourceGroupName = 'elad_arm_template2'
$location = 'uksouth'
$plainpass = 'passw0rd!'
$pfxFilePath = 'c:\temp\cert2.pfx'
$secretName = 'newname'
$vaultName = 'eladsecurecommkv2'
$dnsName = 'testcert.petri.com'
$storageName = 'eladstorage123'
# Create resource group
az group create --name $resourceGroupName --location $location

# Deploys the ARM template
New-AzureRmResourceGroupDeployment -Name arm_first_version -ResourceGroupName $resourceGroupName -TemplateFile template.json -kv_name $vaultName -storage_name $storageName -TemplateParameterFile parameters.json 
#az group deployment create  --name arm_first_version --resource-group $resourceGroupName --template-file template.json --kv_name $vaultName --storage_name $storageName

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
$clearBytes = $collection.Export($pkcs12ContentType)
$fileContentEncoded = [System.Convert]::ToBase64String($clearBytes)
$secret = ConvertTo-SecureString -String $fileContentEncoded -Force -AsPlainText
$secretContentType = 'application/x-pkcs12'
Set-AzureKeyVaultSecret -VaultName $vaultName -Name $secretName -SecretValue $Secret -ContentType $secretContentType

# Delete local file 
#Remove-Item –path $pfxFilePath

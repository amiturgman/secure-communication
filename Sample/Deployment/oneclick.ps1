# This script creates a resource group and deploys into it an Azure KeyVault and AzureStorage.
# Next, it populates the KeyVault with a certificate stored as a secret (pfx - which contains public and private key)

# parameters
$resourceGroupName = '[The resource group name]'
$location = '[The resources location]'
$plainpass = '[Certificate password]'
$pfxFilePath = '[The pfx temporary file]'
$secretName = '[The keyvault secret name]'
$vaultName = '[The vault name]'
$dnsName = '[The certificate dns name, example: testcert.contoso.com]'
$storageName = '[Azure storage name]'
$tenant  = '[Azure tenant id]'
$applicationId= '[Azure application id]'
$key = "[Azure application key]"

# Login to Azure with service principal
$SecurePassword = $key | ConvertTo-SecureString -AsPlainText -Force
$cred = new-object -typename System.Management.Automation.PSCredential -argumentlist $applicationId, $SecurePassword
Add-AzureRmAccount -Credential $cred -Tenant $tenant -ServicePrincipal

# Create resource group
New-AzureRmResourceGroup -Name $resourceGroupName -Location $location

# Deploys the ARM template
New-AzureRmResourceGroupDeployment -ResourceGroupName $resourceGroupName -TemplateFile template.json -kv_name $vaultName -storage_name $storageName -location $location -TemplateParameterFile parameters.json -tenant $tenant

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
Set-AzureRmKeyVaultAccessPolicy -VaultName $vaultName -ServicePrincipalName $applicationId -PermissionsToSecrets set,delete,get,list
Set-AzureKeyVaultSecret -VaultName $vaultName -Name $secretName -SecretValue $Secret -ContentType $secretContentType

# Delete local file 
Remove-Item -path $pfxFilePath

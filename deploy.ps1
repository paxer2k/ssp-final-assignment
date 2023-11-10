#Publish resources via template
$prefix = '647833'

$resourceGroupName = "$prefix-ssp-resource"
$location = 'westeurope'
$serverFarmName = "$prefix-ssp-serverfarm"

$subscriptionId = '8e7c7eb8-572a-4e7b-9a57-9efbf2706e4a'
$bicepTemplatePath = './template.bicep'

$functionAppName = "$prefix-ssp-final-assignment"

# Queue names
$initJobQueueName = 'init-job-queue'
$processResourcesQueueName = 'process-resources-queue'
$editImagesQueueName = 'edit-images-queue'

# Blob container names
$uploadImageContainerName = 'upload-image-container'
$sasLinkContainerContainerName = 'sas-link-container'

# Table names
$jobTableName = 'JobTable'

$parameters = @{
	prefix = $prefix
	serverFarmName = $serverFarmName
	functionAppName = $functionAppName
	initJobQueueName = $initJobQueueName
	processResourcesQueueName = $processResourcesQueueName
	editImagesQueueName = $editImagesQueueName
	uploadImageContainerName = $uploadImageContainerName
	sasLinkContainerContainerName = $sasLinkContainerContainerName
	jobTableName = $jobTableName
}

$parameters = $parameters.Keys.ForEach({"$_=$($parameters[$_])"}) -join ' '

Write-Host "Deploying resources in $resourceGroupName"

# Set the context to your subscription
az account set --subscription $subscriptionId

# Create new resource group (if does not exist)
az group create -l $location -n $resourceGroupName

# Deploy resources inside resource group
$cmd = "az deployment group create --mode Incremental --resource-group $resourceGroupName --template-file $bicepTemplatePath --parameters $parameters"
Write-Host $cmd
Invoke-Expression $cmd


# Publish solution
$cwd = (Get-Location)
$publishDir = "$cwd/Functions/bin/Release/net6.0/publish"
$publishZip = "$cwd/publish.zip"
dotnet publish -c Release

if (Test-path $publishZip) {Remove-Item $publishZip}

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($publishDir, $publishZip)
az functionapp deployment source config-zip --resource-group $resourceGroupName --name $functionAppName --src $publishZip
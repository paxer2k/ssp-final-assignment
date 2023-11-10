// Resource names definition
param prefix string
param location string = resourceGroup().location
param serverFarmName string

// Queue names
param initJobQueueName string
param processResourcesQueueName string
param editImagesQueueName string

// Blob container names
param uploadImageContainerName string
param sasLinkContainerContainerName string 

// Table names
param jobTableName string

// Function name
param functionAppName string

// Random stuff
param tags object = {}
param appInsightsRetention int = 30
param numberOfWorkers int = 1 // why do i need this??

var storageAccountName = '${prefix}sspstorage'

// Storage account definition
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
      supportsHttpsTrafficOnly: true
      allowBlobPublicAccess: true
      minimumTlsVersion: 'TLS1_2'
  }
}

// Application insight for performance tracking
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: storageAccountName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    RetentionInDays: appInsightsRetention
  }
  tags: tags
}

// Queue #1
resource initJobQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: '${storageAccount.name}/default/${initJobQueueName}'
  properties: {}
}

// Queue #2
resource processResourcesQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: '${storageAccount.name}/default/${processResourcesQueueName}'
  properties: {}
}

// Queue 3
resource editImagesQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-01-01' = {
  name: '${storageAccount.name}/default/${editImagesQueueName}'
  properties: {}
}

// Blob container #1
resource uploadImageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/${uploadImageContainerName}'
  properties: {
    publicAccess: 'Blob'
  }
}

// Blob container #2
resource sasLinkContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccount.name}/default/${sasLinkContainerContainerName}'
  properties: {
    publicAccess: 'Blob'
  }
}

// Table storage #1
resource jobTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2023-01-01' = {
  name: '${storageAccount.name}/default/${jobTableName}'
  properties: {}
}

resource serverFarm 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: serverFarmName
  location: location
  tags: tags
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
  kind: 'functionapp'
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: resourceId('Microsoft.Web/serverfarms', serverFarm.name)
    siteConfig: {
      autoHealEnabled: true
      autoHealRules: {
        triggers: {
          privateBytesInKB: 0
          statusCodes: [
            {
              status: 500
              subStatus: 0
              win32Status: 0
              count: 25
              timeInterval: '00:05:00'
            }
          ]
        }
        actions: {
          actionType: 'Recycle'
          minProcessExecutionTime: '00:01:00'
        }
      }
      numberOfWorkers: numberOfWorkers
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }

  resource functionAppConfig 'config@2021-03-01' = {
    name: 'appsettings'
    properties: {
      // function app settings
      'AzureWebJobsStorage': 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(resourceId('Microsoft.Storage/storageAccounts', storageAccount.name), '2021-08-01').keys[0].value};EndpointSuffix=core.windows.net'
      'FUNCTIONS_EXTENSION_VERSION': '~4'
      'FUNCTIONS_WORKER_RUNTIME': 'dotnet-isolated'
      'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING': 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(resourceId('Microsoft.Storage/storageAccounts', storageAccount.name), '2019-04-01').keys[0].value};EndpointSuffix=core.windows.net'
      'WEBSITE_CONTENTSHARE': replace(toLower(functionApp.name), '-', '')
      // ai settings
      'APPINSIGHTS_INSTRUMENTATIONKEY': reference('Microsoft.Insights/components/${appInsights.name}', '2015-05-01').InstrumentationKey
      'ApplicationInsightsAgent_EXTENSION_VERSION': '~2'
      'InstrumentationEngine_EXTENSION_VERSION': '~1'
      'BuienradarConfig:BuienradarDataUrl': 'https://data.buienradar.nl/2.0/feed/json'
      'UnsplashConfig:AccessKey': '-9UpdBoZCybKsWQC-sC4W-otrg_GNColtZazBNrtK6Q'
      'UnsplashConfig:PhotoId': 'Ip32fUzkCVg'
      'JobConfig:ResultEndpoint': 'https://${functionApp.name}/api/GetResultFunction/'
      'TableConfig:JobTable': 'JobTable'
      'QueueConfig:InitJobQueue': 'init-job-queue'
      'QueueConfig:ProcessResourcesQueue': 'process-resources-queue'
      'QueueConfig:EditImagesQueue': 'edit-images-queue'
      'BlobConfig:UploadImageContainer': 'upload-image-container'
      'BlobConfig:SasLinkContainer': 'sas-link-container'
      'BlobConfig:OriginalImageFilename': 'original-image-filename'
    }
  }
}
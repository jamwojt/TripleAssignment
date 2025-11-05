param location string = 'francecentral'
// Storage Account
resource storage 'Microsoft.Storage/storageAccounts@2025-06-01' = {
  name: 'trassignmentstorageacc'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    isHnsEnabled: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-06-01' = {
  name: 'default'
  parent: storage
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-06-01' = {
  name: 'images'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2025-06-01' = {
  name: 'default'
  parent: storage
}

resource startQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2025-06-01' = {
  name: 'start-process'
  parent: queueService
}

resource stationQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2025-06-01' = {
  name: 'station-data'
  parent: queueService
}

// App Service Plan
resource functionPlan 'Microsoft.Web/serverfarms@2025-03-01' = {
  name: 'triple-app-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

// Function App
resource functionApp 'Microsoft.Web/sites@2025-03-01' = {
  name: 'triple-func'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionPlan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
            name: 'AzureWebJobsStorage'
            value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${storage.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
            name: 'FUNCTIONS_EXTENSION_VERSION'
            value: '~4'
        }
      ]
    }
  }
}

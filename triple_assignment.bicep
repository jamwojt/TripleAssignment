param prefix string = 'tripleassignment'

@description('Location for all resources')
param location string = resourceGroup().location

// Storage Account
resource storage 'Microsoft.Storage/storageAccounts@2023-06-01' = {
  name: '${prefix}storageacc'
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

// Blob container
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-06-01' = {
  name: '${storage.name}/default/images'
  properties: {
    publicAccess: 'None'
  }
  dependsOn: [
    storage
  ]
}

// Queues
resource startqueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-06-01' = {
  name: '${storage.name}/default/start-process'
  dependsOn: [storage]
}

resource stationqueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-06-01' = {
  name: '${storage.name}/default/station-data'
  dependsOn: [storage]
}

// App Service Plan
resource functionPlan 'Microsoft.Web/serverfarms@2023-10-01' = {
  name: '${prefix}-plan'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  kind: 'functionapp'
}

// Function App
resource functionApp 'Microsoft.Web/sites@2023-10-01' = {
  name: '${prefix}-func'
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionPlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
      ]
    }
  }
  dependsOn: [
    functionPlan
    storage
  ]
}

dotnet publish -c Release -o ./publish

cd publish
zip -r ../app.zip .
cd ..

az group create \
    --name triple-rg \
    --location francecentral

az deployment group create \
    --resource-group triple-rg \
    --template-file azure_services.bicep

az functionapp deployment source config-zip \
    --name triple-func \
    --resource-group triple-rg \
    --src app.zip

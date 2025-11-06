# Simple Azure Functions App
Gets temperature data for multiple cities and stores the result on the Blob storage.
Results can be retrieved using an http endpoint.

# Deployment
- clone the repository
- Run deploy.ps1 or deploy.sh depending on the OS

# Access
- Request new images with data using StartProcess endpoint
- View generated images using AccessImages endpoint with the city query argument.

For examples, see docs.http file.

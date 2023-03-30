# Login to your Azure account
az login

# Set your Azure subscription ID
az account set --subscription <subscription_id>

# Define your resource group and region
resourceGroup=<resource_group_name>
location=<azure_region>

# Create a resource group
az group create --name $resourceGroup --location $location

# Create a storage account
storageName=<storage_account_name>
az storage account create --name $storageName --resource-group $resourceGroup --location $location --sku Standard_LRS

# Create a Function app in a Linux Consumption Plan
functionAppName=<function_app_name>
az functionapp create --name $functionAppName --resource-group $resourceGroup --storage-account $storageName --os-type Linux --consumption-plan-location $location --runtime dotnet-isolated --runtime-version 7 --functions-version 4

# Allow all networks to access the function app
az functionapp cors add --name $functionAppName --resource-group $resourceGroup --allowed-origins "*"

echo "Script execution completed successfully."

# After the Azure Function is created, you can deploy the code by going to Visual Studio Code,
# then creating an empty folder, and then using the Workspace table from Azure extensions to 
# create a new Azure Function project. Select the "Azure Function App in C#" template and then continue
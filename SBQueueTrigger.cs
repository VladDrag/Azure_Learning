using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Npgsql;

namespace Azure_Learning;

public static class SBQueueTrigger
{
    // private static string _serviceBusConn = (string) GetSecret("ServiceBusConnection");
    public static string GetSecret(string secretName)
    {
        //DOES NOT WORK!!!!!!! MUST CONTINUE TRYING;
        
        // function can be modified by adding vaultUrl as a parameter in the function signature
        var vaultUrl = "https://vlad-id-dev-kv.vault.azure.net/";
        
        // Create a new instance of KeyVaultClient
        var azureServiceTokenProvider = new AzureServiceTokenProvider(); //check if this is correct
        var keyVaultClient = new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

        // Retrieve the specified secret from the Key Vault
        var secret = keyVaultClient.GetSecretAsync(vaultUrl, secretName).GetAwaiter().GetResult();

        return secret.Value;
    }
    
    [FunctionName("SBQueueTrigger")]
    public static async Task RunAsync([ServiceBusTrigger("funcappqueue", Connection = "ServiceBusConnection" )] Message myQueueItem, ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {Encoding.UTF8.GetString(myQueueItem.Body)}");

        var messageContent = Encoding.UTF8.GetString(myQueueItem.Body);
        var result = "vdtest";

        // Insert message into database using Dapper
        
        //We are using local env variables, but just as well we could use the Get Secret function to retrieve the connection string from Key Vault
        var connectionString = Environment.GetEnvironmentVariable("DbConnString");
        // var connectionString = GetSecret("DbConnString");

        log.LogInformation(connectionString);
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            await connection.ExecuteAsync("CREATE TABLE IF NOT EXISTS Messages (Id SERIAL PRIMARY KEY, Message TEXT)");
            await connection.ExecuteAsync("INSERT INTO Messages (Message) VALUES (@Message)",new {Message =  messageContent} );
            var results = await connection.QueryAsync<string>("SELECT * FROM Messages WHERE Id = 1");
            result = results.ToList()[0];
        }
        log.LogInformation("The result is: " + result);
    }
}
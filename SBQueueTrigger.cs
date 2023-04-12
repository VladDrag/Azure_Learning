using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Npgsql;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;

namespace Azure_Learning;

public static class SBQueueTrigger
{
    public async static Task<string> GetSecret(string secretName)
    {
        var kvUrl = $"https://vlad-id-dev-kv.vault.azure.net";
        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        
        var keyVaultClient = new KeyVaultClient( new KeyVaultClient.AuthenticationCallback( azureServiceTokenProvider.KeyVaultTokenCallback));

        var builder = new ConfigurationBuilder();
        builder.AddAzureKeyVault(kvUrl, keyVaultClient, new DefaultKeyVaultSecretManager());
        builder.AddEnvironmentVariables();
        var config = builder.Build();
        return config[secretName];

    }
    
    [FunctionName("SBQueueTrigger")]
    public static async Task RunAsync([ServiceBusTrigger("funcappqueue", Connection = "ServiceBusConnection" )] Message myQueueItem, ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {Encoding.UTF8.GetString(myQueueItem.Body)}");

        var messageContent = Encoding.UTF8.GetString(myQueueItem.Body);
        var result = "vdtest";

        // Insert message into database using Dapper
        
        //We can also use environmental variables
        //var connectionString = Environment.GetEnvironmentVariable("DbConnString");
        var connectionString = await GetSecret("DbConnString");
        log.LogInformation("The SECRET from KEY VAULT IS : " + connectionString);

        //try extract info from App Seeting at Function App
        log.LogInformation("The app setting is : " + await GetSecret("AzureWebJobsStorage"));
        
        //try extract info from Connection String at Function App
        log.LogInformation("The conn string is : " + await GetSecret("ServiceBusConnection"));
        
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
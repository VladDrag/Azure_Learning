using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using Npgsql;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Azure_Learning;

public static class SBQueueTrigger
{
    // private static string _serviceBusConn = (string) GetSecret("ServiceBusConnection");
    public async static Task<string> GetSecret(string secretName)
    {
        var keyVaultName = "vlad-id-dev-kv";
        var kvUri = $"https://{keyVaultName}.vault.azure.net";

        var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        Console.WriteLine($"Retrieving your secret from {keyVaultName}.");
        var secret = await client.GetSecretAsync(secretName);
        Console.WriteLine($"Your secret is '{secret.Value.Value}'.");

        return secret.Value.Value;
        
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
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.ServiceBus;


namespace Azure_Learning
{
    public static class HttpToServiceBus
    {
        [FunctionName("HttpToServiceBus")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            //set connection to service bus
            string connectionString = System.Environment.GetEnvironmentVariable("ServiceBusConnection");

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            //transform data into byte array
            byte[] messageBytes = Encoding.UTF8.GetBytes(responseMessage);

            //select queue for message direction
            string queueName = "funcappqueue";


            //create queue client
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            //create message for queue
            Message message = new Message(messageBytes);

            //send message to queue
            await queueClient.SendAsync(message);

            //optionally return custom message with information of success / failure; in this case, it's the same as the message
            return new OkObjectResult(responseMessage);
        }
    }
}


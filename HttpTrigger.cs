using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
// using Microsoft.Azure.Functions.Worker;
using Newtonsoft.Json;
using Npgsql;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Aibel.Func
{
	public class MyClass
	{
		// public MyClass( )
		// {
		// 	this.table_name = table_name;
		// }
		public string table_name { get; set; }
	}

	public class GetDbData
	{
		public List<T> GetTableNamesAsync<T>(NpgsqlConnection con) where T : MyClass, new()
		{
			var tableNames = con.Query("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'").ToList();
			List<T> list = tableNames.Select(item => new T { table_name = item.table_name }).AsEnumerable().ToList();
			return list;
		}
	}

    public static class HttpTrigger
    {

        [FunctionName("HttpTrigger")]
        public static Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			// var reqData = new StreamReader(req.Body).ReadToEndAsync();
			// var response = reqData.CreateResponse(System.Net.HttpStatusCode.OK);
			// var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
			string connectionString = System.Environment.GetEnvironmentVariable("connString");
			using (var con = new NpgsqlConnection(connectionString)) 
			{
				con.Open(); 
				var dbProvider = new GetDbData();
				List<MyClass> tables = dbProvider.GetTableNamesAsync<MyClass>(con);
				// var tableNames = con.Query("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'").ToList();
				// List<T> list = await tableNames.Select(item => new T { table_name = item.table_name }).AsEnumerable().ToListAsync();

				int tableCount = tables.Count();
				Console.WriteLine("Number of tables is: " + tableCount);
				// response.WriteString("Number of tables is: " + tableCount);
				// var res = new ContentResult
				// {
				// 	Content = "Number of tables is: " + tableCount,
				// 	ContentType = "text/plain",
				// 	StatusCode = 200
				// };
				return Task.FromResult<IActionResult>(new OkObjectResult("Number of tables is: " + tableCount));
			}
        }
    }
}

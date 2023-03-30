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
using Newtonsoft.Json;
using Npgsql;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Aibel.Func
{
	public class MyClass
	{
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
			string connectionString = System.Environment.GetEnvironmentVariable("connString");
			using (var con = new NpgsqlConnection(connectionString)) 
			{
				con.Open(); 
				var dbProvider = new GetDbData();
				List<MyClass> tables = dbProvider.GetTableNamesAsync<MyClass>(con);

				int tableCount = tables.Count();
				return Task.FromResult<IActionResult>(new OkObjectResult("Number of tables is: " + tableCount));
			}
        }
    }
}

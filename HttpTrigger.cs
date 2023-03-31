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
	// public class MyClass
	// {
	// 	public string table_name { get; set; }
	// }

	// public class GetDbData
	// {
	// 	public List<T> GetTableNamesAsync<T>(NpgsqlConnection con) where T : MyClass, new()
	// 	{
	// 		var tableNames = con.Query("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'").ToList();
	// 		List<T> list = tableNames.Select(item => new T { table_name = item.table_name }).AsEnumerable().ToList();
	// 		return list;
	// 	}
	// }

    public static class HttpTrigger
    {

        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			string connectionString = System.Environment.GetEnvironmentVariable("connString");
			using (var con = new NpgsqlConnection(connectionString)) 
			{
				con.Open();
				// create new table
				//con.Execute("CREATE TABLE IF NOT EXISTS test (id serial PRIMARY KEY, name VARCHAR(40) NOT NULL, date TIMESTAMP NOT NULL)");


				// delete table
				//con.Execute("DROP TABLE IF EXISTS test");

				// insert data *ONLY IF WE KNOW A TABLE EXISTS! -> we need to add extra code to check if table exists
				//con.Execute("INSERT INTO test (name, date) VALUES(@Name, @Date)", new { Name = "Tim", Date = DateTime.Now });

				// get data with Dapper
				//var test = con.Query("SELECT * FROM test").ToList();
	
				// possible redundant code
				//var dbProvider = new GetDbData();
				//List<MyClass> tables = dbProvider.GetTableNamesAsync<MyClass>(con);

				// in order to query async we need to use Dapper; 
				// for that, we need to use QueryAsync instead of Query; 
				// we cannot use ToList() with QueryAsync, so we need insert the items in a list after the operation finishes;
				var tableInfo = await con.QueryAsync("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'");
				var tables = tableInfo.ToList();

				//serialization done with Newtonsoft.Json
				var tableInfoJson = JsonConvert.SerializeObject(tables);

				int tableCount = tables.Count();
				return new OkObjectResult("Number of tables is: " + tableCount + " and the table names are: " + tableInfoJson);
			}
        }
    }
}

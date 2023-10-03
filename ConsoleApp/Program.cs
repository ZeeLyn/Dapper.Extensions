using Dapper.Extensions.Factory;
using System;
using System.IO;
using System.Text;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Dapper.Extensions;
using Autofac.Core;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            DapperFactory.CreateInstance().ConfigureServices(service => { })
                .ConfigureContainer(container =>
                {
                    container.AddDapperForSQLite();
                    container.AddDapperForSQLite("Sqlite2", "sqlite2");
                })
                .ConfigureConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json");
                }).Build();

            DapperFactory.Step(dapper =>
            {
                var query = dapper.Query("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            });

            var result7 = DapperFactory.Step(dapper => { return dapper.Query("select * from Contact;"); });
            Console.WriteLine(JsonConvert.SerializeObject(result7));

            DapperFactory.StepAsync(async dapper =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            }).Wait();

            var result8 = DapperFactory.StepAsync(async dapper => await dapper.QueryAsync("select * from Contact;"))
                .Result;
            Console.WriteLine(JsonConvert.SerializeObject(result8));


            DapperFactory.Step("sqlite2", dapper =>
            {
                var query = dapper.Query("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            });

            var result9 = DapperFactory.Step("sqlite2", dapper => { return dapper.Query("select * from Contact;"); });
            Console.WriteLine(JsonConvert.SerializeObject(result9));

            DapperFactory.StepAsync("sqlite2", async dapper =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            }).Wait();

            var result10 = DapperFactory
                .StepAsync("sqlite2", async dapper => await dapper.QueryAsync("select * from Contact;")).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result10));


            DapperFactory.Step(context =>
            {
                var dapper = context.ResolveDapper();
                var query = dapper.Query("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            });

            DapperFactory.StepAsync(async context =>
            {
                var dapper = context.ResolveDapper();
                var queryAsync = await dapper.QueryAsync("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(queryAsync));
            }).Wait();

            DapperFactory.Step((context, dapper) =>
            {
                var query = dapper.Query("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            });

            DapperFactory.StepAsync(async (context, dapper) =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            }).Wait();

            DapperFactory.Step("sqlite2", (context, dapper) =>
            {
                var query = dapper.Query("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            });

            DapperFactory.StepAsync("sqlite2", async (context, dapper) =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Console.WriteLine(JsonConvert.SerializeObject(query));
            }).Wait();

            var result1 = DapperFactory.Step(context =>
            {
                var dapper = context.ResolveDapper();
                return dapper.Query("select * from Contact;");
            });
            Console.WriteLine(JsonConvert.SerializeObject(result1));

            var result2 = DapperFactory.StepAsync(context =>
            {
                var dapper = context.ResolveDapper();
                return dapper.QueryAsync("select * from Contact;");
            }).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result2));

            var result3 = DapperFactory.Step((context, dapper) => dapper.Query("select * from Contact;"));
            Console.WriteLine(JsonConvert.SerializeObject(result3));

            var result4 = DapperFactory
                .StepAsync(async (context, dapper) => await dapper.QueryAsync("select * from Contact;")).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result4));

            var result5 = DapperFactory.Step("sqlite2", (context, dapper) => dapper.Query("select * from Contact;"));
            Console.WriteLine(JsonConvert.SerializeObject(result5));

            var result6 = DapperFactory.StepAsync("sqlite2",
                async (context, dapper) => await dapper.QueryAsync("select * from Contact;")).Result;
            Console.WriteLine(JsonConvert.SerializeObject(result6));

            Console.ReadKey();
        }
    }
}
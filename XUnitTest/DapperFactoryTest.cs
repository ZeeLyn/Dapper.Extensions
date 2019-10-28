using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using Dapper.Extensions;
using Dapper.Extensions.Factory;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest
{

    public class DapperFactoryTest
    {
        private ITestOutputHelper Output { get; }
        public DapperFactoryTest(ITestOutputHelper output)
        {
            Output = output;
            DapperFactory.CreateInstance().ConfigureServices(service =>
            {
                service.AddDapperForSQLite();
            }).ConfigureContainer(container =>
            {
                container.AddDapperForSQLite("Sqlite2", "sqlite2");
                container.AddDapperForSQLite("master_slave", "master_slave", true);
            }).ConfigureConfiguration(builder =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
                builder.AddJsonFile("appsettings.json");
            }).Build();
        }


        [Fact]
        public void Test()
        {
            DapperFactory.Step(dapper =>
            {
                var query = dapper.Query("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            });

            var result7 = DapperFactory.Step(dapper => { return dapper.Query("select * from Contact;"); });
            Assert.NotNull(result7);
            Assert.True(result7.Any());

            DapperFactory.StepAsync(async dapper =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            }).Wait();

            var result8 = DapperFactory.StepAsync(async dapper => await dapper.QueryAsync("select * from Contact;")).Result;
            Assert.NotNull(result8);
            Assert.True(result8.Any());


            DapperFactory.Step("sqlite2", dapper =>
            {
                var query = dapper.Query("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            });

            var result9 = DapperFactory.Step("sqlite2", dapper => { return dapper.Query("select * from Contact;"); });
            Assert.NotNull(result9);
            Assert.True(result9.Any());

            DapperFactory.StepAsync("sqlite2", async dapper =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            }).Wait();

            var result10 = DapperFactory.StepAsync("sqlite2", async dapper => await dapper.QueryAsync("select * from Contact;")).Result;
            Assert.NotNull(result10);
            Assert.True(result10.Any());


            DapperFactory.Step(context =>
            {
                var dapper = context.ResolveDapper();
                var query = dapper.Query("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            });

            DapperFactory.StepAsync(async context =>
            {
                var dapper = context.ResolveDapper();
                var queryAsync = await dapper.QueryAsync("select * from Contact;");
                Assert.NotNull(queryAsync);
                Assert.True(queryAsync.Any());
            }).Wait();

            DapperFactory.Step((context, dapper) =>
            {
                var query = dapper.Query("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            });

            DapperFactory.Step(context =>
            {
                var master = context.ResolveDapper("master_slave");
                var rows = master.Execute("update COMPANY set name=@name where id=@id;", new { name = Guid.NewGuid().ToString(), id = 1 });
                Assert.True(rows > 0);
                Output.WriteLine("Master:" + master.Conn.Value.ConnectionString);


                var slave = context.ResolveDapper("master_slave", true);
                var read = slave.Query("select * from company;");
                Assert.NotNull(read);
                Assert.True(read.Any());
                Output.WriteLine("Slave:" + slave.Conn.Value.ConnectionString);
            });

            DapperFactory.StepAsync(async (context, dapper) =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            }).Wait();

            DapperFactory.Step("sqlite2", (context, dapper) =>
            {
                var query = dapper.Query("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            });

            DapperFactory.StepAsync("sqlite2", async (context, dapper) =>
            {
                var query = await dapper.QueryAsync("select * from Contact;");
                Assert.NotNull(query);
                Assert.True(query.Any());
            }).Wait();

            var result1 = DapperFactory.Step(context =>
            {
                var dapper = context.ResolveDapper();
                return dapper.Query("select * from Contact;");
            });
            Assert.NotNull(result1);
            Assert.True(result1.Any());

            var result2 = DapperFactory.StepAsync(context =>
            {
                var dapper = context.ResolveDapper();
                return dapper.QueryAsync("select * from Contact;");
            }).Result;
            Assert.NotNull(result2);
            Assert.True(result2.Any());

            var result3 = DapperFactory.Step((context, dapper) => dapper.Query("select * from Contact;"));
            Assert.NotNull(result3);
            Assert.True(result3.Any());

            var result4 = DapperFactory.StepAsync(async (context, dapper) => await dapper.QueryAsync("select * from Contact;")).Result;
            Assert.NotNull(result4);
            Assert.True(result4.Any());

            var result5 = DapperFactory.Step("sqlite2", (context, dapper) => dapper.Query("select * from Contact;"));
            Assert.NotNull(result5);
            Assert.True(result5.Any());

            var result6 = DapperFactory.StepAsync("sqlite2", async (context, dapper) => await dapper.QueryAsync("select * from Contact;")).Result;
            Assert.NotNull(result6);
            Assert.True(result6.Any());
        }
    }
}

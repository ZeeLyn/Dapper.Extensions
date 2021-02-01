using Dapper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest
{
    [Collection("TestHostCollection")]
    public class ExecuteTest : TestStartupFixture
    {
        private ITestOutputHelper Output { get; }
        private IServiceProvider Services { get; }
        private IDapper Dapper { get; }
        public ExecuteTest(ITestOutputHelper output, TestStartupFixture fixture)
        {
            Output = output;
            Services = fixture.Services;
            Dapper = Services.GetService<IDapper>();
        }

        [Fact]
        public void Execute()
        {
            var newName = Guid.NewGuid().ToString();
            var rows = Dapper.Execute("update contact set name=@name where id=@id;",
                 new { name = newName, id = 1 });
            Assert.True(rows > 0);
            var name = Dapper.QueryFirstOrDefaultAsync<string>("select name from contact where id=@id;", new { id = 1 }).Result;
            Assert.Equal(name, newName);
        }

        [Fact]
        public void ExecuteAsync()
        {
            var newName = Guid.NewGuid().ToString();
            var rows = Dapper.ExecuteAsync("update contact set name=@name where id=@id;",
                new { name = newName, id = 1 }).Result;
            Assert.True(rows > 0);
            var name = Dapper.QueryFirstOrDefaultAsync<string>("select name from contact where id=@id;", new { id = 1 }).Result;
            Assert.Equal(name, newName);
        }

        [Fact]
        public void ExecuteScalar()
        {
            var newName = Guid.NewGuid().ToString();
            var name = Dapper.ExecuteScalar<string>(
                "update contact set name=@name where id=@id; select name from contact where id=@id;", new { id = 1, name = newName });
            Assert.Equal(name, newName);
        }

        [Fact]
        public void ExecuteScalarAsync()
        {
            var newName = Guid.NewGuid().ToString();
            var name = Dapper.ExecuteScalarAsync<string>(
                "update contact set name=@name where id=@id; select name from contact where id=@id;", new { id = 1, name = newName }).Result;
            Assert.Equal(name, newName);
        }


        [Fact]
        public void Transaction()
        {
            var newName = Guid.NewGuid().ToString();
            Dapper.BeginTransaction();
            var rows = Dapper.Execute("update contact set name=@name where id=@id;",
                new { name = newName, id = 1 });
            Assert.True(rows > 0);
            Dapper.CommitTransaction();
            var name = Dapper.QueryFirstOrDefault<string>("select name from contact where id=@id;", new { id = 1 });
            Assert.Equal(name, newName);

            Dapper.BeginTransaction();
            var oldName = Dapper.QueryFirstOrDefault<string>("select name from contact where id=@id;", new { id = 1 });
            var rows2 = Dapper.Execute("update contact set name=@name where id=@id;",
                new { name = newName, id = 1 });
            Assert.True(rows2 > 0);
            Dapper.RollbackTransaction();
            var nowName = Dapper.QueryFirstOrDefault<string>("select name from contact where id=@id;", new { id = 1 });
            Assert.Equal(oldName, nowName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Dapper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest
{
    [Collection("TestHostCollection")]
    public class MasterSlaveTest : TestStartupFixture
    {
        private ITestOutputHelper Output { get; }
        private IServiceProvider Services { get; }
        private IDapper DapperMaster { get; }
        private IDapper DapperSlave { get; }
        private IResolveContext Context { get; }

        public MasterSlaveTest(ITestOutputHelper output, TestStartupFixture fixture)
        {
            Output = output;
            Services = fixture.Services;
            Context = Services.GetRequiredService<IResolveContext>();

            DapperMaster = Context.ResolveDapper("master_slave");
            DapperSlave = Context.ResolveDapper("master_slave", true);
        }

        [Fact]
        public void Test()
        {

            var rows = DapperMaster.Execute("update COMPANY set name=@name where id=@id;", new { name = Guid.NewGuid().ToString(), id = 1 });
            Assert.True(rows > 0);
            Output.WriteLine("Master:" + DapperMaster.Conn.Value.ConnectionString);

            var read = DapperSlave.Query("select * from company;");
            Assert.NotNull(read);
            Assert.True(read.Any());
            Output.WriteLine("Slave:" + DapperSlave.Conn.Value.ConnectionString);
        }
    }
}

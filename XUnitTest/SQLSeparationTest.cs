using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest
{
    [Collection("TestHostCollection")]
    public class SQLSeparationTest : TestStartupFixture
    {
        private ITestOutputHelper Output { get; }
        private IServiceProvider Services { get; }
        private IDapper Dapper { get; }
        public SQLSeparationTest(ITestOutputHelper output, TestStartupFixture fixture)
        {
            Output = output;
            Services = fixture.Services;
            Dapper = Services.GetService<IDapper>();
        }


        [Fact]
        public void Test()
        {
            var result = Dapper.Query(name: "COMPANY.list.query", new { id = 1 });
            Assert.NotNull(result);

            var page = Dapper.QueryPage(name: "paging", 1, 20);
            Assert.NotNull(page);
            Assert.True(page.Contents.Any());
        }
    }
}

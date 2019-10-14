using System;
using Dapper.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTest
{
    [Collection("TestHostCollection")]
    public class QueryTest : TestStartupFixture
    {
        private ITestOutputHelper Output { get; }
        private IServiceProvider Services { get; }
        private IDapper Dapper { get; set; }
        public QueryTest(ITestOutputHelper output, TestStartupFixture fixture)
        {
            Output = output;
            Services = fixture.Services;
            Dapper = Services.GetService<IDapper>();
        }
        [Fact]
        public void QueryAsync()
        {
            var result = Dapper.QueryAsync<Contact>("select * from Contact;").Result;
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void QueryAsyncMultiMappingWith2()
        {
            var result = Dapper.QueryAsync<Contact, Passport, Contact>("select Contact.id,Contact.name,Passport.ContactId,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id;", (contact, passport) =>
              {
                  contact.Passport = passport;
                  return contact;
              }, null, "ContactId").Result;
            Assert.True(result.Count > 0);
            foreach (var item in result)
            {
                Assert.NotNull(item.Passport);
            }
        }
    }

    public class Contact
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Passport Passport { get; set; }
    }

    public class Passport
    {
        public string PassportNumber { get; set; }
    }
}

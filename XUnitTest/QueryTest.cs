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
        public void Query()
        {
            var result1 = Dapper.QueryAsync<Contact>("select * from Contact;").Result;
            Assert.True(result1.Count > 0);

            var result2 = Dapper.QueryAsync("select * from Contact;").Result;
            Assert.True(result2.Count > 0);

            var result3 = Dapper.Query<Contact>("select * from Contact;");
            Assert.True(result3.Count > 0);

            var result4 = Dapper.Query("select * from Contact;");
            Assert.True(result4.Count > 0);
        }

        [Fact]
        public void QueryMultiMappingWith2()
        {
            var result1 = Dapper.QueryAsync<Contact, Passport, Contact>("select Contact.id,Contact.name,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id;", (contact, passport) =>
              {
                  contact.Passport = passport;
                  return contact;
              }, null, "PassportNumber").Result;
            Assert.True(result1.Count > 0);
            foreach (var item in result1)
            {
                Assert.NotNull(item.Passport);
            }

            var result2 = Dapper.Query<Contact, Passport, Contact>("select Contact.id,Contact.name,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id;", (contact, passport) =>
            {
                contact.Passport = passport;
                return contact;
            }, null, "PassportNumber");
            Assert.True(result2.Count > 0);
            foreach (var item in result2)
            {
                Assert.NotNull(item.Passport);
            }
        }

        [Fact]
        public void QueryFirstOrDefault()
        {
            var result1 = Dapper.QueryFirstOrDefaultAsync<Contact>("select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;", new { id = 1 }).Result;
            Assert.NotNull(result1);

            var result2 = Dapper.QueryFirstOrDefault<Contact>("select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;", new { id = 1 });
            Assert.NotNull(result2);


            var result3 = Dapper.QueryFirstOrDefaultAsync("select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;", new { id = 1 }).Result;
            Assert.NotNull(result3);

            var result4 = Dapper.QueryFirstOrDefault("select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;", new { id = 1 });
            Assert.NotNull(result4);
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

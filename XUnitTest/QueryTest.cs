using System;
using System.Linq;
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
            var result1 = Dapper.QueryAsync<Contact>("select * from Contact;", enableCache: true).Result;
            Assert.True(result1.Count > 0);

            var temp = Dapper.QueryAsync<Contact>("select * from Contact;", enableCache: true).Result;

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

            var result5 = Dapper.QueryFirstOrDefault("select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id in @id;", new { id = new[] { 1, 2, 3 } });
        }

        [Fact]
        public void QuerySingleOrDefault()
        {
            var result1 = Dapper.QuerySingleOrDefaultAsync<Contact>("select id from Contact where id=@id;", new { id = 1 }).Result;
            Assert.NotNull(result1);

            var result2 = Dapper.QuerySingleOrDefault<Contact>("select id from Contact where id=@id;", new { id = 1 });
            Assert.NotNull(result2);


            var result3 = Dapper.QuerySingleOrDefaultAsync("select id,name from Contact where id=@id;", new { id = 1 }).Result;
            Assert.NotNull(result3);

            var result4 = Dapper.QuerySingleOrDefault("select id from Contact where id=@id;", new { id = 1 });
            Assert.NotNull(result4);

            Assert.Throws<InvalidOperationException>(() =>
            {
                Dapper.QuerySingleOrDefault("select id from Contact where id in @id;", new { id = new[] { 1, 2, 3 } });
            });
        }

        [Fact]
        public void QueryMultiple()
        {
            Dapper.QueryMultiple("select * from Contact;select * from Passport;", async (reader) =>
               {
                   var contacts = await reader.ReadAsync<Contact>();
                   Assert.True(contacts.Any());

                   var passports = await reader.ReadAsync<Passport>();

                   Assert.True(passports.Any());
               });

            Dapper.QueryMultipleAsync("select * from Contact;select * from Passport;", async (reader) =>
            {
                var contacts = await reader.ReadAsync<Contact>();
                Assert.True(contacts.Any());

                var passports = await reader.ReadAsync<Passport>();
                Assert.True(passports.Any());
            }).Wait();

            var result2 = Dapper.QueryMultipleAsync<Contact, Passport>("select * from Contact;select * from Passport;").Result;
            Assert.True(result2.Result1.Any());
            Assert.True(result2.Result2.Any());

            var result3 = Dapper.QueryMultipleAsync<Contact, Passport, Contact>("select * from Contact;select * from Passport;select * from Contact;").Result;
            Assert.True(result3.Result1.Any());
            Assert.True(result3.Result2.Any());
            Assert.True(result3.Result3.Any());

            var result4 = Dapper.QueryMultipleAsync<Contact, Passport, Contact, Passport>("select * from Contact;select * from Passport;select * from Contact;select * from Passport;").Result;
            Assert.True(result4.Result1.Any());
            Assert.True(result4.Result2.Any());
            Assert.True(result4.Result3.Any());
            Assert.True(result4.Result4.Any());

            var result5 = Dapper.QueryMultipleAsync<Contact, Passport, Contact, Passport, Contact>("select * from Contact;select * from Passport;select * from Contact;select * from Passport;select * from Contact;").Result;
            Assert.True(result5.Result1.Any());
            Assert.True(result5.Result2.Any());
            Assert.True(result5.Result3.Any());
            Assert.True(result5.Result4.Any());
            Assert.True(result5.Result5.Any());
        }

        [Fact]
        public void ExecuteReader()
        {
            using var reader1 = Dapper.ExecuteReader("select * from Contact;");
            while (reader1.Read())
            {
                Assert.True(reader1.GetInt32(0) > 0);
            }

            using var reader2 = Dapper.ExecuteReaderAsync("select * from Contact;").Result;
            while (reader2.Read())
            {
                Assert.True(reader2.GetInt32(0) > 0);
            }
        }

        [Fact]
        public void QueryPage()
        {
            var result1 = Dapper.QueryPage("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 1);
            Assert.NotNull(result1);
            Assert.True(result1.Contents.Count == 1);

            var result2 = Dapper.QueryPageAsync("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 2).Result;
            Assert.NotNull(result2);
            Assert.True(result2.Contents.Count == 2);

            var result3 = Dapper.QueryPage<Contact>("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 1);
            Assert.NotNull(result3);
            Assert.True(result3.Contents.Count == 1);

            var result4 = Dapper.QueryPageAsync<Contact>("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 2).Result;
            Assert.NotNull(result4);
            Assert.True(result4.Contents.Count == 2);

            Assert.Throws<ArgumentException>(() =>
            {
                Dapper.QueryPage("select count(*) from contact;",
                    "select * from contact limit @Take OFFSET @Skip;", 0, 1);
            });

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await Dapper.QueryPageAsync("select count(*) from contact;",
                      "select * from contact limit @Take OFFSET @Skip;", 0, 2);
            });
            Assert.Throws<ArgumentException>(() =>
            {
                Dapper.QueryPage<Contact>("select count(*) from contact;",
                    "select * from contact limit @Take OFFSET @Skip;", 0, 1);
            });

            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await Dapper.QueryPageAsync<Contact>("select count(*) from contact;",
                    "select * from contact limit @Take OFFSET @Skip;", 0, 2);
            });
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

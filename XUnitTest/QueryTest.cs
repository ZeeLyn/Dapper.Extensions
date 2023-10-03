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
        public async void Query()
        {
            var result1 = await Dapper.QueryAsync<Contact>("select * from Contact;", enableCache: true);
            Assert.True(result1.Count > 0);

            var temp = await Dapper.QueryAsync<Contact>("select * from Contact;", enableCache: true);

            var result2 = await Dapper.QueryAsync("select * from Contact;");
            Assert.True(result2.Count > 0);

            var result3 = Dapper.Query<Contact>("select * from Contact;");
            Assert.True(result3.Count > 0);

            var result4 = Dapper.Query("select * from Contact;");
            Assert.True(result4.Count > 0);
        }

        [Fact]
        public async void QueryMultiMappingWith2()
        {
            var result1 = await Dapper.QueryAsync<Contact, Passport, Contact>(
                "select Contact.id,Contact.name,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id;",
                (contact, passport) =>
                {
                    contact.Passport = passport;
                    return contact;
                }, null, "PassportNumber");
            Assert.True(result1.Count > 0);
            foreach (var item in result1)
            {
                Assert.NotNull(item.Passport);
            }

            var result2 = Dapper.Query<Contact, Passport, Contact>(
                "select Contact.id,Contact.name,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id;",
                (contact, passport) =>
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
        public async void QueryFirstOrDefault()
        {
            var result1 = await Dapper.QueryFirstOrDefaultAsync<Contact>(
                "select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;",
                new { id = 1 });
            Assert.NotNull(result1);

            var result2 = Dapper.QueryFirstOrDefault<Contact>(
                "select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;",
                new { id = 1 });
            Assert.NotNull(result2);


            var result3 = await Dapper
                .QueryFirstOrDefaultAsync(
                    "select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;",
                    new { id = 1 });
            Assert.NotNull(result3);

            var result4 = Dapper.QueryFirstOrDefault(
                "select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id=@id;",
                new { id = 1 });
            Assert.NotNull(result4);

            var result5 = Dapper.QueryFirstOrDefault(
                "select Contact.name,Contact.id,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id and Contact.id in @id;",
                new { id = new[] { 1, 2, 3 } });
        }

        [Fact]
        public async void QuerySingleOrDefault()
        {
            var result1 = await Dapper
                .QuerySingleOrDefaultAsync<Contact>("select id from Contact where id=@id;", new { id = 1 });
            Assert.NotNull(result1);

            var result2 = Dapper.QuerySingleOrDefault<Contact>("select id from Contact where id=@id;", new { id = 1 });
            Assert.NotNull(result2);


            var result3 =
                    await Dapper.QuerySingleOrDefaultAsync("select id,name from Contact where id=@id;", new { id = 1 })
                ;
            Assert.NotNull(result3);

            var result4 = Dapper.QuerySingleOrDefault("select id from Contact where id=@id;", new { id = 1 });
            Assert.NotNull(result4);

            Assert.Throws<InvalidOperationException>(() =>
            {
                Dapper.QuerySingleOrDefault("select id from Contact where id in @id;",
                    new { id = new[] { 1, 2, 3 } });
            });
        }

        [Fact]
        public async void QueryMultiple()
        {
            Dapper.QueryMultiple("select * from Contact;select * from Passport;", async (reader) =>
            {
                var contacts = await reader.ReadAsync<Contact>();
                Assert.True(contacts.Any());

                var passports = await reader.ReadAsync<Passport>();

                Assert.True(passports.Any());
            });

            await Dapper.QueryMultipleAsync("select * from Contact;select * from Passport;", async (reader) =>
            {
                var contacts = await reader.ReadAsync<Contact>();
                Assert.True(contacts.Any());

                var passports = await reader.ReadAsync<Passport>();
                Assert.True(passports.Any());
            });

            var result2 =
                    await Dapper.QueryMultipleAsync<Contact, Passport>("select * from Contact;select * from Passport;")
                ;
            Assert.True(result2.Result1.Any());
            Assert.True(result2.Result2.Any());

            var result3 = await Dapper
                .QueryMultipleAsync<Contact, Passport, Contact>(
                    "select * from Contact;select * from Passport;select * from Contact;");
            Assert.True(result3.Result1.Any());
            Assert.True(result3.Result2.Any());
            Assert.True(result3.Result3.Any());

            var result4 = await Dapper.QueryMultipleAsync<Contact, Passport, Contact, Passport>(
                "select * from Contact;select * from Passport;select * from Contact;select * from Passport;");
            Assert.True(result4.Result1.Any());
            Assert.True(result4.Result2.Any());
            Assert.True(result4.Result3.Any());
            Assert.True(result4.Result4.Any());

            var result5 = await Dapper.QueryMultipleAsync<Contact, Passport, Contact, Passport, Contact>(
                    "select * from Contact;select * from Passport;select * from Contact;select * from Passport;select * from Contact;")
                ;
            Assert.True(result5.Result1.Any());
            Assert.True(result5.Result2.Any());
            Assert.True(result5.Result3.Any());
            Assert.True(result5.Result4.Any());
            Assert.True(result5.Result5.Any());
        }

        [Fact]
        public async void ExecuteReader()
        {
            using var reader1 = Dapper.ExecuteReader("select * from Contact;");
            while (reader1.Read())
            {
                Assert.True(reader1.GetInt32(0) > 0);
            }

            using var reader2 = await Dapper.ExecuteReaderAsync("select * from Contact;");
            while (reader2.Read())
            {
                Assert.True(reader2.GetInt32(0) > 0);
            }
        }

        [Fact]
        public async void QueryPage()
        {
            var result1 = Dapper.QueryPage("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 1);
            Assert.NotNull(result1);
            Assert.True(result1.Contents.Count == 1);

            var result2 = await Dapper.QueryPageAsync("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 2);
            Assert.NotNull(result2);
            Assert.True(result2.Contents.Count == 2);

            var result3 = Dapper.QueryPage<Contact>("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 1);
            Assert.NotNull(result3);
            Assert.True(result3.Contents.Count == 1);

            var result4 = await Dapper.QueryPageAsync<Contact>("select count(*) from contact;",
                "select * from contact limit @Take OFFSET @Skip;", 1, 2);
            Assert.NotNull(result4);
            Assert.True(result4.Contents.Count == 2);

            var result5 =
                    await Dapper.QueryPlainPageAsync<Contact>("select * from contact limit @Take OFFSET @Skip;", 1, 2)
                ;
            Assert.NotNull(result5);
            Assert.True(result5.Count == 2);

            var result6 = await Dapper.QueryPlainPageAsync("select * from contact limit @Take OFFSET @Skip;", 1, 2);
            Assert.NotNull(result6);
            Assert.True(result6.Count == 2);

            var result7 = Dapper.QueryPlainPage<Contact>("select * from contact limit @Take OFFSET @Skip;", 1, 2);
            Assert.NotNull(result7);
            Assert.True(result7.Count == 2);

            var result8 = Dapper.QueryPlainPage("select * from contact limit @Take OFFSET @Skip;", 1, 2);
            Assert.NotNull(result8);
            Assert.True(result8.Count == 2);

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
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using StackExchange.StacMan;
using Stacky;
using System;

namespace RavenPlayground.Console
{
    public class Class1
    {
        static void Main(string[] args)
        {
            string certPassword = Environment.GetEnvironmentVariable("certPassword");
            string certLocation = Environment.GetEnvironmentVariable("certLocation");
            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { "https://a.zitherit.ravendb.community:8443" },
                Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certLocation, certPassword),
                Database = "test"
            }.Initialize();

            using (IDocumentSession testSession = store.OpenSession())
            {
                testSession.Store(new Employee
                {
                    FirstName = "John",
                    LastName = "Doe"
                });

                // send all pending operations to server, in this case only `Put` operation
                testSession.SaveChanges();
            }

            var client = new StacManClient(key: "v3N0Lx)RV7ef4XfkVEzaTQ((", version: "2.2");
            var response = client.Questions.GetAll("stackoverflow",
            page: 1,
            pagesize: 5,
            sort: StackExchange.StacMan.Questions.AllSort.Creation,
            order: Order.Desc,
            tagged: "c#;asp.net-core;asp.net"
            //filter: "!*1NqAXBn(skZn5Uym67oyOymEdBNJB2sTPd_.AyXM"
            ).Result;

            foreach (var question in response.Data.Items)
            {
                System.Console.WriteLine(question.Title);
            }

            /*using (BulkInsertOperation bulkInsert = store.BulkInsert())
            {
            for (int i = 0; i < 1000 * 1000; i++)
            {
            bulkInsert.Store(new Employee
            {
            FirstName = "FirstName #" + i,
            LastName = "LastName #" + i
            });
            }
            }
            */

            /*store.Maintenance.Send(new PutIndexesOperation(new IndexDefinitionBuilder<Employee>("Employee/FirstName")
            {
            Map = posts => from post in Employees
            select new
            {
            post.Tags,
            post.Content
            },
            Analyzers =
            {
            {x => x.FirstName, "SimpleAnalyzer"},
            {x => x.LastName, typeof(SnowballAnalyzer).AssemblyQualifiedName}
            }
            }.ToIndexDefinition(store.Conventions)));
            */

            store.Dispose();
            System.Console.WriteLine("Hello World!");

        }



        internal class Employee

        {

            public string FirstName { get; set; }

            public string LastName { get; set; }

        }
    }
}

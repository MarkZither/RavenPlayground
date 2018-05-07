using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Documents.Session;
using StackExchange.StacMan;
using System.Linq;
using System;
using System.Net;
using System.Security;
using System.Threading;

namespace RavenPlayground.Console
{
    public class Class1
    {
        static void Main(string[] args)
        {
            SecureString certPassword = new NetworkCredential("", Environment.GetEnvironmentVariable("certPassword")).SecurePassword;
            string certLocation = Environment.GetEnvironmentVariable("certLocation");
            string dbServer = Environment.GetEnvironmentVariable("ravenDBServer");
            

            System.Console.WriteLine($"Using certificate {certLocation}");
            System.Console.WriteLine($"Using RavenDB at {dbServer}");
            System.Console.WriteLine($"Please enter the database name");
            string databaseName = System.Console.ReadLine();

            IDocumentStore store = new DocumentStore()
            {
                Urls = new[] { dbServer },
                Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certLocation, certPassword),
                Database = databaseName
            }.Initialize();

            System.Console.WriteLine("What do you want to do?");
            System.Console.WriteLine("To get some data from stackoverflow in the database press S");
            System.Console.WriteLine("To create a full text index over the stackoverflow questions press I");
            System.Console.WriteLine("To add a test record press T");
            var key = System.Console.ReadKey();
            if (key.Key.Equals(ConsoleKey.T))
            {
                AddTestRecord(store);
            }
            else if (key.Key.Equals(ConsoleKey.S))
            {
                System.Console.WriteLine("How many pages (100 questions per page) of data do you want?");
                int.TryParse(System.Console.ReadLine(), out int pages);
                AddStackoverflowData(store, pages);
            }
            else if (key.Key.Equals(ConsoleKey.I))
            {
                AddIndexesAndAnalyzers(store);
            }
            else
            {
                System.Console.WriteLine("try again");
            }

            store.Dispose();
            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadKey();
        }

        private static void AddIndexesAndAnalyzers(IDocumentStore store)
        {
            /*
             * with this index you can search like
             * from index 'Question/SearchByTitle'
             * where search(Title, "asp")
             */
            store.Maintenance.Send(new PutIndexesOperation(new IndexDefinitionBuilder<Question>("Question/SearchByTitle")
            {
                Map = questions => from question in questions
                                   select new
                                   {
                                       question.Title
                                   },
                Analyzers =
                {
                   {x => x.Title, typeof(StandardAnalyzer).AssemblyQualifiedName}
                }
            }.ToIndexDefinition(store.Conventions)));


            store.Maintenance.Send(new PutIndexesOperation(new IndexDefinitionBuilder<Question>("Question/SearchByTitleAndBody")
            {
                Map = questions => from question in questions
                                   select new
                                   {
                                       question.Title,
                                       question.Body,
                                       Query = new[]
                                        {
                                   question.Title,
                                   question.Body
                                       }
                                   },
                Analyzers =
                {
                   {x => x.Title, typeof(StandardAnalyzer).AssemblyQualifiedName},
                   {x => x.Body, typeof(StandardAnalyzer).AssemblyQualifiedName},
                }
            }.ToIndexDefinition(store.Conventions)));

            new Question_Query().Execute(store);
            new Questions_ByTitleAndBody().Execute(store);
        }

        private static void AddStackoverflowData(IDocumentStore store, int pages)
        {
            string stackAppsKey = Environment.GetEnvironmentVariable("stackAppsKey");
            int page = 1;
            bool hasMore = true;
            while (hasMore)
            {
                var client = new StacManClient(key: stackAppsKey, version: "2.2");
                var response = client.Questions.GetAll("stackoverflow",
                page: page,
                pagesize: 100,
                sort: StackExchange.StacMan.Questions.AllSort.Activity,
                order: Order.Asc,
                tagged: "python",
                //filter: "!9Z(-wwYGT", // question with body 
                //filter: "!9Z(-wwK0y", //question with body and body_markdown
                filter: "!LVBj29mB6Y(88vxKX2ObLu" //with comments and answers bodies
                ).Result;

                foreach (var question in response.Data.Items)
                {
                    System.Console.WriteLine(question.Title);
                }

                using (BulkInsertOperation bulkInsert = store.BulkInsert())
                {
                    foreach (var question in response.Data.Items)
                    {
                        bulkInsert.Store(question);
                    }
                }
                if (response.Data.Backoff.HasValue)
                {
                    Thread.Sleep(response.Data.Backoff.Value * 2000);
                }
                Thread.Sleep(1000);
                hasMore = response.Data.HasMore && page < pages;

                page++;
            }
        }

        private static void AddTestRecord(IDocumentStore store)
        {
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
        }

        internal class Employee
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        /// <summary>
        /// with this index you can search with
        /// from index 'Question/Query'
        /// where search(Query, 'asp')
        /// </summary>
        public class Question_Query : AbstractIndexCreationTask<Question>
        {
            public class Result
            {
                public string[] Query { get; set; }
            }

            public Question_Query()
            {
                Map = questions => from question in questions
                                   select new
                                   {
                                       Query = new[]
                                        {
                                    question.Title,
                                    question.Body
                                }
                                   };

                Index("Query", FieldIndexing.Search);
            }
        }

        public class Questions_ByTitleAndBody : AbstractIndexCreationTask<Question>
        {
            public class Result
            {
                public string TitleAndBody { get; set; }
            }

            public Questions_ByTitleAndBody()
            {
                Map = questions => from question in questions
                                   select new
                                   {
                                       TitleAndBody = question.Title + " " + question.Body
                                   };

                Analyze("TitleAndBody", typeof(StandardAnalyzer).AssemblyQualifiedName);
            }
        }
    }
}
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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using RavenPlayground.Console.Models;

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
            System.Console.WriteLine("To index project gutenberg data press G");
            System.Console.WriteLine("To create a full text index over the project gutenberg data press P");
            System.Console.WriteLine("To add a test record press T");
            
            var key = System.Console.ReadKey();
            System.Console.WriteLine();
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
            else if (key.Key.Equals(ConsoleKey.G))
            {
                System.Console.WriteLine("Where is the Project Gutenberg root folder? (Default D:\\gut)");
                string PGLoc = System.Console.ReadLine();
                if(string.IsNullOrEmpty(PGLoc))
                {
                    PGLoc = "D:\\gut";
                }
                AddProjectGutenbergData(store, PGLoc);
            }
            else
            {
                System.Console.WriteLine("try again");
            }

            store.Dispose();
            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadKey();
        }

        private static void AddProjectGutenbergData(IDocumentStore store, string pGLoc)
        {
            
            //find all the zip files
            var zipFiles = Directory.GetFiles(pGLoc, "*.zip", SearchOption.AllDirectories);
            Stream unzippedEntryStream;
            foreach (var zipPath in zipFiles)
            {
                using (ZipArchive archive = System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        {
                            FileInfo file = new FileInfo(entry.FullName);
                            unzippedEntryStream = entry.Open(); // .Open will return a stream
                                                                // Process entry data here
                            StreamReader reader = new StreamReader(unzippedEntryStream);
                            string text = reader.ReadToEnd();
                            try
                            {
                                IGraph g = new Graph();
                                IGraph h = new Graph();

                                string parsedBookId = Path.GetFileNameWithoutExtension(entry.Name).Contains("-") ? Path.GetFileNameWithoutExtension(entry.Name).Substring(0, Path.GetFileNameWithoutExtension(entry.Name).IndexOf("-")) : Path.GetFileNameWithoutExtension(entry.Name);
                                int bookId = Convert.ToInt32(parsedBookId);
                                //Load using Filename
                                FileLoader.Load(h, pGLoc + $"\\rdf-files\\cache\\epub\\{parsedBookId}\\pg{parsedBookId}.rdf");

                                TripleStore tstore = new TripleStore();

                                //Assume that we fill our Store with data from somewhere

                                //Create a dataset for our queries to operate over
                                //We need to explicitly state our default graph or the unnamed graph is used
                                //Alternatively you can set the second parameter to true to use the union of all graphs
                                //as the default graph
                                InMemoryDataset ds = new InMemoryDataset(tstore, new Uri("http://purl.org/dc/terms"));

                                //Get the Query processor
                                ISparqlQueryProcessor processor = new LeviathanQueryProcessor(ds);
                                NTriplesFormatter formatter = new NTriplesFormatter();
                                foreach (Triple t in h.Triples)
                                {
                                    System.Console.WriteLine(t.ToString(formatter));
                                }
                                var nodes = h.GetTriplesWithPredicate(new Uri("http://purl.org/dc/terms/title")).First().Nodes.ToList();
                                var title = h.GetTriplesWithPredicate(new Uri("http://purl.org/dc/terms/title")).First().Nodes.ToList()[2].ToString();
                                var titlestring = h.GetTriplesWithPredicate(new Uri("http://purl.org/dc/terms/title")).First().Nodes.FirstOrDefault();

                                //var creatornodes = h.GetTriplesWithPredicate(new Uri("http://purl.org/dc/terms/creator")).First().Nodes.ToList();
                                var creatorName = h.GetTriplesWithPredicate(new Uri("http://www.gutenberg.org/2009/pgterms/name")).First().Nodes.ToList()[2].ToString();

                                var languagenodes = h.GetTriplesWithPredicate(new Uri("http://purl.org/dc/terms/language")).First().Nodes;
                                //var language = languagenodes.ToList()[2].Graph.GetTriplesWithPredicate(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#")).First().Nodes.ToList()[2].ToString();

                                GutBook book = new GutBook()
                                {
                                    Author = creatorName,
                                    BookId = bookId,
                                    Text = text,
                                    Title = title,
                                    Language = "en"
                                };
                                var Session = store.OpenSession();
                                var existingBook = from d in Session.Query<GutBook>()
                                                      where d.BookId.Equals(bookId)
                                                      select d;
                                List<GutBook> gutBooks = Session
                                    .Query<GutBook>()
                                    .Where(x => x.BookId == bookId)
                                    .ToList();
                                if (gutBooks == null || gutBooks.Count().Equals(0))
                                {
                                    using (BulkInsertOperation bulkInsert = store.BulkInsert())
                                    {
                                        //foreach (var question in response.Data.Items)
                                        //{
                                        bulkInsert.Store(book);
                                        //}
                                    }
                                }


                            }
                            catch (RdfParseException parseEx)
                            {
                                //This indicates a parser error e.g unexpected character, premature end of input, invalid syntax etc.
                                System.Console.WriteLine("Parser Error");
                                System.Console.WriteLine(parseEx.Message);
                            }
                            catch (RdfException rdfEx)
                            {
                                //This represents a RDF error e.g. illegal triple for the given syntax, undefined namespace
                                System.Console.WriteLine("RDF Error");
                                System.Console.WriteLine(rdfEx.Message);
                            }
                        }
                    }
                }
            }
            var something = "";
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
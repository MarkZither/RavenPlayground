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
                StackOverflow.AddData(store, pages);
            }
            else if (key.Key.Equals(ConsoleKey.I))
            {
                StackOverflow.AddIndexesAndAnalyzers(store);
            }
            else if (key.Key.Equals(ConsoleKey.G))
            {
                System.Console.WriteLine("Press D to get data and I to add index");
                key = System.Console.ReadKey();
                System.Console.WriteLine();
                if (key.Key.Equals(ConsoleKey.D))
                {
                    System.Console.WriteLine("Where is the Project Gutenberg root folder? (Default D:\\gut)");
                    string PGLoc = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(PGLoc))
                    {
                        PGLoc = "D:\\gut";
                    }
                    ProjectGutenberg.AddData(store, PGLoc);
                }
                else if (key.Key.Equals(ConsoleKey.I))
                {
                    ProjectGutenberg.AddIndexesAndAnalyzers(store);
                }
            }
            else
            {
                System.Console.WriteLine("try again");
            }

            store.Dispose();
            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadKey();
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
    }
}
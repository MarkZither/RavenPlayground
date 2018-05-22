using Lucene.Net.Analysis.Standard;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Indexes;
using RavenPlayground.Console.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace RavenPlayground.Console
{
    public class ProjectGutenberg
    {
        public static void AddData(IDocumentStore store, string pGLoc)
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

        public static void AddIndexesAndAnalyzers(IDocumentStore store)
        {
            new GutBook_ByTitleAuthorAndBody().Execute(store);
        }

        public class GutBook_ByTitleAuthorAndBody : AbstractIndexCreationTask<GutBook>
        {
            public class Result
            {
                public string TitleAuthorAndBody { get; set; }
            }

            public GutBook_ByTitleAuthorAndBody()
            {
                Map = gutBooks => from gutBook in gutBooks
                                  select new
                                  {
                                      TitleAuthorAndBody = gutBook.Title + " " + gutBook.Author + " " + gutBook.Text
                                  };

                Analyze("TitleAuthorAndBody", typeof(StandardAnalyzer).AssemblyQualifiedName);
            }
        }
    }
}

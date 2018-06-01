using Lucene.Net.Analysis.Standard;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations.Attachments;
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
				try
				{
					using (ZipArchive archive = System.IO.Compression.ZipFile.Open(zipPath, ZipArchiveMode.Read))
					{
						foreach (ZipArchiveEntry entry in archive.Entries)
						{
							if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									FileInfo file = new FileInfo(entry.FullName);
									unzippedEntryStream = entry.Open(); // .Open will return a stream
																		// Process entry data here
									Stream newStream = new MemoryStream();
									unzippedEntryStream.CopyTo(newStream);
									StreamReader reader = new StreamReader(newStream, System.Text.Encoding.UTF8, true);
									reader.BaseStream.Position = 0;
									string text = reader.ReadToEnd();
									reader.Close();
									reader.Dispose();

									IGraph g = new Graph();
									IGraph h = new Graph();

									string parsedBookId = Path.GetFileNameWithoutExtension(entry.Name).Contains("-") ? Path.GetFileNameWithoutExtension(entry.Name).Substring(0, Path.GetFileNameWithoutExtension(entry.Name).IndexOf("-")) : Path.GetFileNameWithoutExtension(entry.Name);
									if (Int32.TryParse(parsedBookId, out int bookId))
									{
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
												book.Version = 1.0;
												bulkInsert.Store(book);
											}
										}
										else
										{
											// do a patch
											string id = Session.Advanced.GetDocumentId(gutBooks.First());
											GutBook oldBook = Session.Load<GutBook>(id);
											if (string.IsNullOrEmpty(oldBook.Text))
											{
												oldBook.Text = text;
												oldBook.Version = oldBook.Version + 0.1;
												AttachmentName[] attachmentNames = Session.Advanced.Attachments.GetNames(oldBook);
												foreach (AttachmentName attachmentName in attachmentNames)
												{
													string name = attachmentName.Name;
													string contentType = attachmentName.ContentType;
													string hash = attachmentName.Hash;
													long size = attachmentName.Size;
												}
												Stream attachStream = new MemoryStream();
												unzippedEntryStream.CopyTo(attachStream);
												Session.Advanced.Attachments.Store(id, $"{oldBook.BookId}.txt", attachStream, "text/plain");
											}
										}
										
										Session.SaveChanges();
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
								catch (Exception ex)
								{
									System.Console.WriteLine("Unexpected Error");
									System.Console.WriteLine(ex.Message);
								}
							}
						}
					}
                }
				catch(Exception ex)
				{
					System.Console.WriteLine("Unexpected Error");
					System.Console.WriteLine(ex.Message);
				}
            }
            var something = "";
        }

        internal static IList<GutBook> Query(IDocumentStore store, string keywords, bool isOr)
        {
            using (var session = store.OpenSession())
            {
                IList<GutBook> results;
                if (isOr)
                {
                    results = session
                        .Query<GutBook, GutBook_ByTitleAuthorAndBody>()
                        .Search(x => x.Query, keywords, options: SearchOptions.Or)
                        .ToList();
                }
                else
                {
                    results = session
                        .Query<GutBook, GutBook_ByTitleAuthorAndBody>()
                        .Search(x => x.Query, keywords, options: SearchOptions.And)
                        .ToList();
                }
                return results;
            }
        }

        public static void AddIndexesAndAnalyzers(IDocumentStore store)
        {
            new GutBook_ByTitleAuthorAndBody().Execute(store);
        }

        /// <summary>
        /// search can look like this
        /// from index 'GutBook/ByTitleAuthorAndBody'
        /// where search(TitleAuthorAndBody, '*Lucius* Annaeus*', and)
        ///  or 
        /// from index 'GutBook/ByTitleAuthorAndBody'
        /// where search(TitleAuthorAndBody, '*Lucius* Annaeus*', or)
        /// or
        /// from index 'GutBook/ByTitleAuthorAndBody'
        /// where search(Query, '*Lucius* Annaeus*', and)
        ///  or 
        /// from index 'GutBook/ByTitleAuthorAndBody'
        /// where search(Query, '*Lucius* Annaeus*', or)
        /// </summary>
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
                                      Query = new object[]
                            {
                                gutBook.Text,
                                gutBook.Author,
                                gutBook.Text,
                            },
                                      TitleAuthorAndBody = gutBook.Title + " " + gutBook.Author + " " + gutBook.Text
                                  };

                Analyze("TitleAuthorAndBody", typeof(StandardAnalyzer).AssemblyQualifiedName);
                Analyze("Query", typeof(StandardAnalyzer).AssemblyQualifiedName);
            }
        }
    }
}

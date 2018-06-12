using Lucene.Net.Analysis.Standard;
using Raven.Client.Documents;
using Raven.Client.Documents.BulkInsert;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations.Indexes;
using StackExchange.StacMan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RavenPlayground.Lib
{
    public class StackOverflow
    {
        public static void AddData(IDocumentStore store, int pages)
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

        public static void AddIndexesAndAnalyzers(IDocumentStore store)
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

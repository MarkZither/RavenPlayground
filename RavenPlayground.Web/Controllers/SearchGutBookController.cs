using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using RavenPlayground.Lib;
using RavenPlayground.Lib.Models;
using RavenPlayground.Web.ViewModels;

namespace RavenPlayground.Web.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SearchGutBookController : ControllerBase
	{
		// GET: api/GutBook/keywords
		[HttpGet("{keywords}")]
		public IEnumerable<GutBook> Get(string keywords)
		{
			string certLocation = Environment.GetEnvironmentVariable("certLocation");
			string dbServer = Environment.GetEnvironmentVariable("ravenDBServer");

			System.Console.WriteLine($"Using certificate {certLocation}");
			System.Console.WriteLine($"Using RavenDB at {dbServer}");
			System.Console.WriteLine($"Please enter the database name");
			string databaseName = "Test"; ;

			SecureString certPassword = new NetworkCredential("", Environment.GetEnvironmentVariable("certPassword")).SecurePassword;
			IDocumentStore store = new DocumentStore()
			{
				Urls = new[] { dbServer },
				Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certLocation, certPassword),
				Database = databaseName
			}.Initialize();

			IList<GutBook> gutBooks = new List<GutBook>();
			gutBooks = ProjectGutenberg.Query(store, keywords, true);

			return gutBooks.Take(10);
		}

		// GET: api/GutBook/keywords
		[HttpGet("Model", Name = "GetModel")]
		public IEnumerable<GutBook> GetModel([FromQuery]SearchParameters model)
		{
			string certLocation = Environment.GetEnvironmentVariable("certLocation");
			string dbServer = Environment.GetEnvironmentVariable("ravenDBServer");

			System.Console.WriteLine($"Using certificate {certLocation}");
			System.Console.WriteLine($"Using RavenDB at {dbServer}");
			System.Console.WriteLine($"Please enter the database name");
			string databaseName = "Test"; ;

			SecureString certPassword = new NetworkCredential("", Environment.GetEnvironmentVariable("certPassword")).SecurePassword;
			IDocumentStore store = new DocumentStore()
			{
				Urls = new[] { dbServer },
				Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certLocation, certPassword),
				Database = databaseName
			}.Initialize();

			IList<GutBook> gutBooks = new List<GutBook>();
			gutBooks = ProjectGutenberg.Query(store, model.Keywords, model.IsOrSearch);

			return gutBooks;
		}
	}
}

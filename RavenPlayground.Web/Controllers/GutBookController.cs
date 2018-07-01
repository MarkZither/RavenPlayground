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

namespace RavenPlayground.Web.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class GutBookController : ControllerBase
  {
    // GET: api/GutBook
    [HttpGet]
    public IEnumerable<GutBook> Get()
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
      gutBooks = ProjectGutenberg.Get(store, 10);

      return gutBooks;
    }

    // GET: api/GutBook/5
    [HttpGet("{id}", Name = "Get")]
    public GutBook Get(int id)
    {
      return new GutBook() { BookId = 1, Author = "mark", Language = "en", Title = "a great book" };
    }

    // POST: api/GutBook
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT: api/GutBook/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE: api/ApiWithActions/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            return new GutBook[] { new GutBook() {BookId = 1, Author = "mark", Language = "en", Title = "a great book" }, new GutBook() { BookId = 2, Author = "dave", Language = "en", Title = "another great book" } };
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

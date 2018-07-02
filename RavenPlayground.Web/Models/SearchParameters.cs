using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RavenPlayground.Web.ViewModels
{
	public class SearchParameters
	{
		public string Keywords { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }
		public bool IsOrSearch { get; set; }
	}
}

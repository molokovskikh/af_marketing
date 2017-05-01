using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class MnnCatalogViewModel
	{
		public uint MnnId { get; set; }
		public uint CatalogId { get; set; }
		public uint CatalogNameId { get; set; }
		public uint CatalogFormId { get; set; }
		public string CatalogName { get; set; }
		public string CatalogForm { get; set; }
	}
}
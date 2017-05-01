using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class ReplacementsGridViewModel
	{
		public string Name { get; set; }
		public string Action { get; set; }
		public IList<ReplacementViewModel> List { get; set; }
	}
}
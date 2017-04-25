using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class AssociationGridModel
	{
		public uint AssociationId { get; set; }
		public string Name { get; set; }
		public string Contacts { get; set; }
		public string Regions { get; set; }
		public string Comments { get; set; }
	}
}
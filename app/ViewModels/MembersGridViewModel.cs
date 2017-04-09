using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class MembersGridViewModel
	{
		[Required]
		public uint MemberId { get; set; }
		[Required]
		public string Name { get; set; }
		public string Region { get; set; }
		[Required]
		public int AddressCount { get; set; }
		public string Subscribes { get; set; }

		public string Contacts { get; set; }
	}
}
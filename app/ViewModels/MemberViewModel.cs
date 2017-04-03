using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marketing.ViewModels
{
	public class MemberViewModel
	{
		[Required]
		[Display(Name = "Участник")]
		public uint MemberId { get; set; }

		public IList<SelectListItem> AvailableMembers { get; set; }
	}
}
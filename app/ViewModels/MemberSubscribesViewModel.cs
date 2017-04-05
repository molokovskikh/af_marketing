using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class MemberSubscribesViewModel
	{
		public uint MemberId { get; set; }
		public string MemberName { get; set; }
		public IList<MemberSubscribe> Promotions { get; set; }
	}

	public class MemberSubscribe
	{
		public uint PromotionId { get; set; }
		public string PromotionName { get; set; }
		public bool Selected { get; set; }
	}
}
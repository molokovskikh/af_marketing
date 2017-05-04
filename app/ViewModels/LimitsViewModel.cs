using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class LimitsViewModel
	{
		public LimitsViewModel()
		{
			MemberLimits = new List<MemberLimitViewModel>();
			LegalEntityLimits = new List<LegalEntityLimitViewModel>();
			AddressLimits = new List<AddressLimitViewModel>();
		}

		public uint PromotionId { get; set; }
		public IList<MemberLimitViewModel> MemberLimits { get; set; }
		public IList<LegalEntityLimitViewModel> LegalEntityLimits { get; set; }
		public IList<AddressLimitViewModel> AddressLimits { get; set; }
	}

	public class MemberLimitViewModel
	{
		public uint MemberId { get; set; }
		public string MemberName { get; set; }
		public decimal? MinSum { get; set; }
	}

	public class LegalEntityLimitViewModel
	{
		public uint LimitId { get; set; }
		public uint MemberId { get; set; }
		public string MemberName { get; set; }
		public string LegalEntityName { get; set; }
		public decimal? MinSum { get; set; }
	}

	public class AddressLimitViewModel
	{
		public uint LimitId { get; set; }
		public uint MemberId { get; set; }
		public string MemberName { get; set; }
		public string Address { get; set; }
		public decimal? MinSum { get; set; }
	}
}
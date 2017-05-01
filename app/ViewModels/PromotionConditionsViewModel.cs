using Marketing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class PromotionConditionsViewModel
	{
		public ProducerPromotion Promotion { get; set; }
		public MarketingEvent MarketingEvent { get; set; }
		public IList<ConditionsGridViewModel> Conditions { get; set; }
	}

	public class ConditionsGridViewModel
	{
		public uint PromotionId { get; set; }
		public uint ConditionId { get; set; }
		public string ProductName { get; set; }
		public string ProducerName { get; set; }
		public uint MnnId { get; set; }
		public string Mnn { get; set; }
		public string Replacements { get; set; }

		[Range(0, 1000000, ErrorMessage = "Цена не может быть отрицательной")]
		public decimal Price { get; set; }

		[Range(0, 100, ErrorMessage = "Процент должен лежать в диапазоне от {0} до {1}")]
		public decimal DealerPercent { get; set; }

		[Range(0, 100, ErrorMessage = "Процент должен лежать в диапазоне от {0} до {1}")]
		public decimal MemberPercent { get; set; }
	}
}
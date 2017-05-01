using Marketing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class EditReplacementsViewModel
	{
		public EditReplacementsViewModel()
		{
			AvailableReplacements = new List<ReplacementViewModel>();
			SelectedReplacements = new List<ReplacementViewModel>();
		}

		public PromotionProduct Condition { get; set; }
		public string SelectedAvailableIds { get; set; }
		public IList<ReplacementViewModel> AvailableReplacements { get; set; }
		public string SelectedReplacementIds { get; set; }
		public IList<ReplacementViewModel> SelectedReplacements { get; set; }
	}
}
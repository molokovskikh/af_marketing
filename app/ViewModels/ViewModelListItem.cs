using System;

namespace Marketing.ViewModels
{
	public interface IViewModelListItem
	{
		ulong Value { get; set; }
		string Text { get; set; }
		bool Selected { get; set; }
	}

	public class ViewModelListItem : IViewModelListItem
	{
		public virtual ulong Value { get; set; }
		public virtual string Text { get; set; }
		public virtual bool Selected { get; set; }
	}

	public class ViewModelRegionListItem : IViewModelListItem
	{
		public virtual ulong Value { get; set; }
		public virtual string Text { get; set; }
		public virtual string Region { get; set; }
		public virtual ulong RegionId { get; set; }
		public virtual bool Selected { get; set; }
	}
}
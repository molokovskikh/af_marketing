using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Ассоциации производителей, участвующих в маркетинговых мероприятиях
	/// </summary>
	public class Association
	{
		public Association()
		{
			Members = new List<PromotionMember>();
			MarketingEvents = new List<MarketingEvent>();
			Promoters = new List<PromoterAssociation>();
			Regions = new List<AssociationRegion>();
			Contacts = new List<AssociationContact>();
		}

		public virtual uint Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string Comments { get; set; }
		public virtual IList<PromotionMember> Members { get; set; }
		public virtual IList<MarketingEvent> MarketingEvents { get; set; }
		public virtual IList<PromoterAssociation> Promoters { get; set; }
		public virtual IList<AssociationRegion> Regions { get; set; }
		public virtual IList<AssociationContact> Contacts { get; set; }
	}
}
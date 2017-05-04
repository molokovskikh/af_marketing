using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности Клиент
	/// </summary>
	public class Client
	{
		public Client()
		{
			ContactGroups = new List<ContactGroup>();
			Addresses = new List<Address>();
			Payers = new List<PayerClient>();
		}

		public virtual uint Id { get; set; }

		public virtual bool Status { get; set; }

		public virtual string Name { get; set; }

		public virtual string FullName { get; set; }

		public virtual uint ContactGroupOwnerId { get; set; }
		public virtual IList<ContactGroup> ContactGroups { get; set; }

		public virtual Region Region { get; set; }

		public virtual IList<Address> Addresses { get; set; }

		public virtual IList<PayerClient> Payers { get; set; }
	}
}
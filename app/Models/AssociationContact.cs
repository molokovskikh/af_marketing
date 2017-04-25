using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Контактная информация по ассоциации производителей
	/// </summary>
	public class AssociationContact
	{
		public virtual uint Id { get; set; }
		public virtual Association Association { get; set; }
		public virtual AssociationContactType ContactType { get; set; }
		public virtual string Fio { get; set; }
		public virtual string Phone { get; set; }
		public virtual string Email { get; set; }

		public static IList<ContactTypeListItem> ContactTypeList()
		{
			return new List<ContactTypeListItem> {
				new ContactTypeListItem {Value = AssociationContactType.Chief, Text = "Руководитель"},
				new ContactTypeListItem {Value = AssociationContactType.Active, Text = "Рабочий"},
				new ContactTypeListItem {Value = AssociationContactType.IT, Text = "IT-специалисты"}
			};
		}
	}

	public enum AssociationContactType : byte
	{
		[Description("Руководитель")] Chief = 0,
		[Description("Рабочий")] Active,
		[Description("IT-специалисты")] IT
	}

	public class ContactTypeListItem
	{
		public AssociationContactType Value { get; set; }
		public string Text { get; set; }
	}
}
using System.ComponentModel.DataAnnotations;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущностти Производитель
	/// </summary>
	public class Producer
	{
		public virtual uint Id { get; set; }

		[Display(Name = "Наименование")]
		public virtual string Name { get; set; }
	}
}
using System;

namespace Marketing.Models
{
	/// <summary>
	///   Заглушка для сущности Регион
	/// </summary>
	public class Region
	{
		public virtual ulong Id { get; set; }

		public virtual string Name { get; set; }
		public virtual bool DrugsSearchRegion { get; set; }
	}
}
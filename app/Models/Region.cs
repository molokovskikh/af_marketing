using System;

namespace Marketing.Models
{
	/// <summary>
	///   Заглушка для сущности Регион
	/// </summary>
	public class Region
	{
		public const ulong INFOROOM_CODE = 524288;

		public virtual ulong Id { get; set; }

		public virtual string Name { get; set; }
		public virtual bool DrugsSearchRegion { get; set; }
	}
}
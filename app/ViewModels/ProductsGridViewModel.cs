using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class ProductsGridViewModel
	{
		[Display(Name = "Код товара")]
		public uint ProductId { get; set; }

		[Display(Name = "Код производителя")]
		public uint ProducerId { get; set; }

		[Display(Name = "Оригинальное Наименование товара")]
		public string ProductName { get; set; }

		[Display(Name = "Оригинальное Наименование Производителя")]
		public string ProducerName { get; set; }

		[Display(Name = "Каталожное Наименование товара")]
		public string CatalogName { get; set; }

		[Display(Name = "Каталожная форма выпуска и дозировка")]
		public string CatalogFormName { get; set; }

		[Display(Name = "Каталожное свойство")]
		public string CatalogProperty { get; set; }

		[Display(Name = "Каталожный Производитель")]
		public string CatalogProducer { get; set; }

		[Display(Name = "Главный Каталожный Производитель")]
		public string MainCatalogProducer { get; set; }

		[Display(Name = "Цех.упак.")]
		public string Package { get; set; }

		[Display(Name = "Кратность")]
		public int Multiplier { get; set; }

		[Display(Name = "Примечание")]
		public string Comment { get; set; }

		[Display(Name = "Документ")]
		public string Document { get; set; }

		[Display(Name = "Признак ЖВ")]
		public bool VitallyImportant { get; set; }
	}
}

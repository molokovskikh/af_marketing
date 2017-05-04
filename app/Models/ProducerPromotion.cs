using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using NHibernate;
using NHibernate.Linq;
using System.ComponentModel;

namespace Marketing.Models
{
	/// <summary>
	/// Акция производителя
	/// </summary>
	public class ProducerPromotion
	{

		public ProducerPromotion()
		{
			Products = new List<PromotionProduct>();
			Suppliers = new List<PromotionSupplier>();
			Subscribes = new List<PromotionSubscribe>();
			FeePercents = new List<PromotionPercent>();
			FeeSums = new List<PromotionSum>();
		}

		public virtual uint Id { get; set; }

		[Required]
		[Display(Name = "Наименование")]
		public virtual string Name { get; set; }

		public virtual MarketingEvent MarketingEvent { get; set; }

		public virtual IList<PromotionProduct> Products { get; set; }

		public virtual IList<PromotionSupplier> Suppliers { get; set; }

		public virtual IList<PromotionSubscribe> Subscribes { get; set; }

		[Required]
		[DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
		[Display(Name = "Дата начала")]
		public virtual DateTime DateStarted { get; set; }

		[Required]
		[DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
		[Display(Name = "Дата окончания")]
		public virtual DateTime DateFinished { get; set; }

		[Required]
		[Display(Name = "В работе")]
		public virtual bool Enabled { get; set; }

		[Required]
		[Display(Name = "Поставщики акции")]
		public virtual SuppliersType SuppliersType { get; set; }

		[Display(Name = "Требования по выкладке акционного товара")]
		public virtual string PromoRequirements { get; set; }

		[Display(Name = "Краткое описание акции")]
		[StringLength(150, ErrorMessage = "Поле {0} не может содержать больше {1} символов")]
		public virtual string Description { get; set; }

		[Display(Name = "Информация о вознаграждении")]
		[StringLength(150, ErrorMessage = "Поле {0} не может содержать больше {1} символов")]
		public virtual string FeeInformation { get; set; }

		[Display(Name = "Форма вознаграждения")]
		public virtual FeeType FeeType { get; set; }

		[Display(Name = "Единица расчёта")]
		public virtual CalculationUnit CalculationUnit { get; set; }

		[Display(Name = "Вознаграждение")]
		public virtual FeeBase FeeBase { get; set; }

		[Display(Name = "Ограничения по минимальному объёму")]
		public virtual LimitType MinLimit { get; set; }

		[Display(Name = "Единые условия для всех товаров")]
		public virtual bool SameConditions { get; set; }

		[Display(Name = "Отчётность")]
		public virtual AccountingPeriod Accounting { get; set; }

		public virtual IList<PromotionPercent> FeePercents { get; set; }
		public virtual IList<PromotionSum> FeeSums { get; set; }

		public virtual void UpdateProductsAndSuppliersByIds(ISession dbSession, string productsIds, string suppliersIds)
		{
			var productsListRaw = (productsIds ?? "").Split(',').Select(s => {
				uint itemId = 0;
				uint.TryParse(s, out itemId);
				return itemId;
			}).Where(s => s != 0).ToList();

			foreach (var item in productsListRaw) {
				if (!Products.Select(s => s.Product.Id).Any(s => s == item)) {
					var baseItem = dbSession.Query<Product>().First(s => s.Id == item);
					var newItem = new PromotionProduct {
						Promotion = this,
						Product = baseItem
					};
					dbSession.Save(newItem);
					Products.Add(newItem);
				}
			}

			var itemsToDelete_Products = Products.Where(s => !productsListRaw.Any(f => f == s.Product.Id)).ToList();
			itemsToDelete_Products.ForEach(s=>Products.Remove(s));

			if (SuppliersType == SuppliersType.All) {
				Suppliers.Clear();
			} else {
				var suppliersListRaw = (suppliersIds ?? "").Split(',').Select(s => {
					uint itemId = 0;
					uint.TryParse(s, out itemId);
					return itemId;
				}).Where(s => s != 0).ToList();

				foreach (var item in suppliersListRaw) {
					if (!Suppliers.Select(s => s.Supplier.Id).Any(s => s == item)) {
						var baseItem = dbSession.Query<Supplier>().First(s => s.Id == item);
						var newItem = new PromotionSupplier {
							Promotion = this,
							Supplier = baseItem
						};
						dbSession.Save(newItem);
						Suppliers.Add(newItem);
					}
				}

				var itemsToDelete_Suppliers = Suppliers.Where(s => !suppliersListRaw.Any(f => f == s.Supplier.Id)).ToList();
				itemsToDelete_Suppliers.ForEach(s => Suppliers.Remove(s));
			}

			dbSession.Save(this);
		}
	}

	public enum SuppliersType : byte
	{
		[Description("Все поставщики")] All = 0,
		[Description("Разрешённые поставщики")] Enabled,
		[Description("Исключения по поставщикам")] Disabled
	}

	/// <summary>
	/// Форма вознаграждения
	/// </summary>
	public enum FeeType : byte
	{
		[Description("Деньгами")] Money = 0,
		[Description("Баллами-Картами")] Cards
	}

	/// <summary>
	/// Единица расчёта
	/// </summary>
	public enum CalculationUnit : byte
	{
		[Description("Учётная цена")] CipPrice = 0,
		[Description("Закупочная цена")] PurchasePrice,
		[Description("Упаковки")] Packages
	}

	/// <summary>
	/// Базовая величина для расчёта вознаграждения
	/// </summary>
	public enum FeeBase : byte
	{
		[Description("Процент от объёма")] Percentage = 0,
		[Description("Сумма за пакет")] Amount
	}

	/// <summary>
	/// Ограничения по минимальному объёму
	/// </summary>
	public enum LimitType : byte
	{
		[Description("Нет")] None = 0,
		[Description("На клиента")] ByClient,
		[Description("На юр.лицо")] ByLegalEntity,
		[Description("На аптеку")] ByAddress
	}

	/// <summary>
	/// Периодичность отчётности
	/// </summary>
	public enum AccountingPeriod : byte
	{
		[Description("Помесячно")] Monthly = 0,
		[Description("Поквартально")] Quarterly
	}
}
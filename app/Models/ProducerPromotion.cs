using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using NHibernate;
using NHibernate.Linq;

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

		public virtual void UpdateProductsAndSuppliersByIds(ISession dbSession, string productsIds, string suppliersIds)
		{
			var productsListRaw = (productsIds ?? "").Split(',').Select(s => {
				uint itemId = 0;
				uint.TryParse(s, out itemId);
				return itemId;
			}).Where(s => s != 0).ToList();
			var suppliersListRaw = (suppliersIds ?? "").Split(',').Select(s => {
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

			dbSession.Save(this);
		}
	}
}
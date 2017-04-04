using System;
using System.Linq;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;

namespace Marketing.ViewModels
{
	public class PromotionViewModel
	{
		public ProducerPromotion Promotion { get; set; }
		public string ProductsListToSetList { get; set; }
		public string SuppliersListToSetList { get; set; }

		public PromotionTableSelectorViewModel ProductListGet { get; set; }
		public PromotionTableSelectorViewModel ProductListSet { get; set; }
		public PromotionTableSelectorViewModel SupplierListGet { get; set; }
		public PromotionTableSelectorViewModel SupplierListSet { get; set; }

		public void SetData(ISession dbSession, uint promotionId)
		{
			ProductListGet = new PromotionTableSelectorViewModel();
			ProductListSet = new PromotionTableSelectorViewModel();
			SupplierListGet = new PromotionTableSelectorViewModel();
			SupplierListSet = new PromotionTableSelectorViewModel();

			Promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
			var currentProducts = string.Join(",", Promotion.Products.Select(s => s.Product.Id).ToList());
			var currentSupplier = string.Join(",", Promotion.Suppliers.Select(s => s.Supplier.Id).ToList());
			ProductListGet.SetData(dbSession, promotionId, PromotionTableSelectorViewModel.RequestType.ProductsListToGet, currentProducts);
			ProductListSet.SetData(dbSession, promotionId, PromotionTableSelectorViewModel.RequestType.ProductsListToSet, currentProducts);
			SupplierListGet.SetData(dbSession, promotionId, PromotionTableSelectorViewModel.RequestType.SuppliersListToGet, currentSupplier);
			SupplierListSet.SetData(dbSession, promotionId, PromotionTableSelectorViewModel.RequestType.SuppliersListToSet, currentSupplier);
			ProductsListToSetList = string.Join(",", ProductListSet.ItemsList.Select(s=>s.Value));
			SuppliersListToSetList = string.Join(",", SupplierListSet.ItemsList.Select(s => s.Value));
		}
	}
}
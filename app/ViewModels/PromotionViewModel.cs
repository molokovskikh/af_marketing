using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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

		public PromotionTableSelectorViewModel<ViewModelListItem> ProductListGet { get; set; }
		public PromotionTableSelectorViewModel<ViewModelListItem> ProductListSet { get; set; }
		public PromotionTableSelectorViewModel<ViewModelRegionListItem> SupplierListGet { get; set; }
		public PromotionTableSelectorViewModel<ViewModelRegionListItem> SupplierListSet { get; set; }
		public List<ViewModelListItem> RegionList { get; set; }

		public void SetData(ISession dbSession, uint promotionId)
		{
			ProductListGet = new PromotionTableSelectorViewModel<ViewModelListItem>();
			ProductListSet = new PromotionTableSelectorViewModel<ViewModelListItem>();
			SupplierListGet = new PromotionTableSelectorViewModel<ViewModelRegionListItem>();
			SupplierListSet = new PromotionTableSelectorViewModel<ViewModelRegionListItem>();
			//RegionList = dbSession.Query<Region>().OrderBy(s => s.Name).Select(s => new SelectListItem() { Text = s.Name, Value = s.Id.ToString() }).ToList();
			RegionList = new List<ViewModelListItem>();
			Promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
			var currentProducts = string.Join(",", Promotion.Products.Select(s => s.Product.Id).ToList());
			var currentSupplier = string.Join(",", Promotion.Suppliers.Select(s => s.Supplier.Id).ToList());
			ProductListGet.SetData(dbSession, promotionId, PromotionTableRequestType.ProductsListToGet, currentProducts, "");
			ProductListSet.SetData(dbSession, promotionId, PromotionTableRequestType.ProductsListToSet, currentProducts, "");
			SupplierListGet.SetData(dbSession, promotionId, PromotionTableRequestType.SuppliersListToGet, currentSupplier, "");
			SupplierListSet.SetData(dbSession, promotionId, PromotionTableRequestType.SuppliersListToSet, currentSupplier, "");
			ProductsListToSetList = string.Join(",", ProductListSet.ItemsList.Select(s=>s.Value));
			SuppliersListToSetList = string.Join(",", SupplierListSet.ItemsList.Select(s => s.Value));
		}
	}
}
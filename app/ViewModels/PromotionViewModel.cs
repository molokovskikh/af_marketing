using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;
using System.ComponentModel;

namespace Marketing.ViewModels
{
	public class PromotionViewModel
	{
		public PromotionViewModel()
		{
			Method = SelectMethod.SelectFromList;
		}

		public SelectMethod Method { get; set; }
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

	public class PromotionFromPriceViewModel
	{
		public PromotionFromPriceViewModel()
		{
			Method = SelectMethod.SelectFromPrice;
			AvailablePrices = new List<PricesGridViewModel>();
			AvailableProducts = new List<ProductsGridViewModel>();
		}

		public SelectMethod Method { get; set; }
		public uint MarketingEventId { get; set; }
		public string MarketingEventName { get; set; }
		public uint PromotionId { get; set; }
		public string PromotionName { get; set; }
		public IList<Producer> Producers { get; set; }
		public string SelectedSupplierIds { get; set; }
		public IList<Supplier> AvailableSuppliers { get; set; }
		public string SelectedPriceIds { get; set; }
		public IList<PricesGridViewModel> AvailablePrices { get; set; }
		public string SelectedProductIds { get; set; }
		public IList<ProductsGridViewModel> AvailableProducts { get; set; }

		public void SetData(ISession dbSession, uint promotionId)
		{
			var promotion = dbSession.Query<ProducerPromotion>()
				.First(r => r.Id == promotionId);
			MarketingEventId = promotion.MarketingEvent.Id;
			MarketingEventName = promotion.MarketingEvent.Name;
			PromotionId = promotionId;
			PromotionName = promotion.Name;
			Producers = promotion.MarketingEvent.Producers.Select(r => r.Producer).ToList();
			SelectedSupplierIds = string.Join(",", promotion.Suppliers.Select(r => r.Supplier.Id.ToString()).ToArray());
			SelectedProductIds = string.Join(",", promotion.Products.Select(r => r.Product.Id.ToString()).ToArray());
			AvailableSuppliers = dbSession.Query<Supplier>()
				.Where(r => !r.Disabled)
				.OrderBy(r => r.Name)
				.ToList();

			var producerIds = string.Join(",", Producers.Select(r => r.Id.ToString()).ToArray());
			if (!string.IsNullOrEmpty(SelectedSupplierIds) && !string.IsNullOrEmpty(producerIds)) {
				var sql = $@"select pd.PriceCode as PriceId, pd.PriceName as Name, pi.PriceDate
	from usersettings.pricesdata pd
		inner join usersettings.pricescosts pc on pc.PriceCode = pd.pricecode
		inner join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
	where pd.Enabled = 1
		and pd.FirmCode in ({SelectedSupplierIds})
		and pd.PriceCode in (
			select c0.PriceCode
				from Farm.Core0 c0
					inner join Catalogs.products p on c0.ProductId = p.Id
					inner join Catalogs.catalog c on p.CatalogId = c.Id
					inner join Catalogs.Assortment a on a.CatalogId = c.Id
				where p.Hidden = 0
					and a.ProducerId in ({producerIds})
			)";
				AvailablePrices = dbSession.Connection.Query<PricesGridViewModel>(sql).ToList();
			}

			SelectedPriceIds = string.Join(",", AvailablePrices.Select(r => r.PriceId).ToArray());
			if (!string.IsNullOrEmpty(producerIds) && !string.IsNullOrEmpty(SelectedPriceIds)) {
				var sql =
					$@"select distinct c0.ProductId, a.ProducerId, c.Name as ProductName, pr.Name as ProducerName,
		cn.Name as CatalogName, cf.Form as CatalogFormName, p.Properties as CatalogProperty,
		pr.Name as CatalogProducer, pr.Name as MainCatalogProducer, '' as Package,
		1 as Multiplier, '' as `Comment`, '' as Document, c.VitallyImportant
	from Farm.Core0 c0
		inner join Catalogs.products p on c0.ProductId = p.Id
		inner join Catalogs.catalog c on p.CatalogId = c.Id
		inner join Catalogs.Assortment a on a.CatalogId = c.Id
		inner join Catalogs.Producers pr on a.ProducerId = pr.Id
		inner join Catalogs.CatalogNames cn on c.NameId = cn.Id
		inner join Catalogs.CatalogForms cf on c.FormId = cf.Id
	where p.Hidden = 0
		and a.ProducerId in ({producerIds})
		and c0.PriceCode in ({SelectedPriceIds})";
				AvailableProducts = dbSession.Connection.Query<ProductsGridViewModel>(sql).ToList();
			}
		}
	}

	public enum SelectMethod : byte
	{
		[Description("Выбор из списка")] SelectFromList,
		[Description("Выбор из прайса")] SelectFromPrice
	}
}
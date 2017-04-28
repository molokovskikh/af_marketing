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
		public SuppliersType SuppliersType { get; set; }
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
			SuppliersType = Promotion.SuppliersType;
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
		public uint SupplierId { get; set; }
		public string SupplierName { get; set; }
		public IList<Producer> Producers { get; set; }
		public string SelectedPriceIds { get; set; }
		public IList<PricesGridViewModel> AvailablePrices { get; set; }
		public string SelectedProductIds { get; set; }
		public IList<ProductsGridViewModel> AvailableProducts { get; set; }

		public void Init(ISession dbSession, uint promotionId)
		{
			var promotion = dbSession.Query<ProducerPromotion>()
				.First(r => r.Id == promotionId);
			MarketingEventId = promotion.MarketingEvent.Id;
			MarketingEventName = promotion.MarketingEvent.Name;
			PromotionId = promotionId;
			PromotionName = promotion.Name;
			SupplierId = promotion.MarketingEvent.Association.Supplier.Id;
			SupplierName = promotion.MarketingEvent.Association.Supplier.Name;
			Producers = promotion.MarketingEvent.Producers.Select(r => r.Producer).ToList();
			SelectedProductIds = string.Join(",", promotion.Products.Select(r => r.Product.Id.ToString()).ToArray());

			var producerIds = string.Join(",", Producers.Select(r => r.Id.ToString()).ToArray());
			var regionMask = (ulong?)promotion.MarketingEvent.Association.Regions.Select(r => r.Region.Id).Distinct().Sum(x => (decimal)x);
			if (regionMask.HasValue)
				AvailablePrices = GetPricesList(dbSession, producerIds, (ulong)regionMask);

			SelectedPriceIds = string.Join(",", AvailablePrices.Select(r => r.PriceId).ToArray());
			AvailableProducts = GetProductsList(dbSession, producerIds, SelectedPriceIds);
		}

		public IList<PricesGridViewModel> GetPricesList(ISession dbSession, string producerIds, ulong regionMask)
		{
			if (string.IsNullOrEmpty(producerIds))
				return new List<PricesGridViewModel>();

			var sql = $@"select pd.PriceCode as PriceId, pd.PriceName as Name, pi.PriceDate,
		group_concat(distinct r.Region order by r.Region separator ', ') as Regions
	from usersettings.pricesdata pd
		inner join usersettings.pricescosts pc on pc.PriceCode = pd.pricecode
		inner join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
		left join Farm.Regions r on pd.RegionMask = r.RegionCode or (pd.RegionMask & r.RegionCode) > 0
	where pd.Enabled = 1
		and pd.FirmCode = {SupplierId}
		and (pd.RegionMask = 0 or (pd.RegionMask & {regionMask}) > 0)
		and pd.PriceCode in (
			select c0.PriceCode
				from Farm.Core0 c0
					inner join Catalogs.products p on c0.ProductId = p.Id
					inner join Catalogs.catalog c on p.CatalogId = c.Id
					inner join Catalogs.Assortment a on a.CatalogId = c.Id
				where p.Hidden = 0
					and a.ProducerId in ({producerIds})
			)
	group by pd.PriceCode
	order by pi.PriceDate desc, pd.PriceName";
			return dbSession.Connection.Query<PricesGridViewModel>(sql).ToList();
		}

		public IList<ProductsGridViewModel> GetProductsList(ISession dbSession, string producerIds, string priceIds)
		{
			if (string.IsNullOrEmpty(producerIds) || string.IsNullOrEmpty(priceIds))
				return new List<ProductsGridViewModel>();

			var sql =
				$@"select c0.ProductId, c0.Code, c0.CodeCr, ifnull(s.Synonym, c.Name) as ProductName,
		ifnull(sf.Synonym, pr.Name) as ProducerName, cn.Name as CatalogName, cf.Form as CatalogFormName,
		(select cast(
				group_concat(ifnull(cpv.Value, '')
					order by cp.PropertyName, cpv.Value
					separator ', ')
				as char)
			from Catalogs.ProductProperties cpp
				left join Catalogs.PropertyValues cpv on cpv.Id = cpp.PropertyValueId
				left join Catalogs.Properties cp on cp.Id = cpv.PropertyId
			where cpp.ProductId = p.Id
		) as CatalogProperty,
		ifnull(ppr.Name, '') as CatalogProducer, ifnull(ppr.Name, '') as MainCatalogProducer, c0.Unit as Package,
		c0.RequestRatio as Multiplier, c0.Note as `Comment`, c0.Doc as Document, c.VitallyImportant
	from Farm.Core0 c0
		inner join Catalogs.Products p on c0.ProductId = p.Id
		inner join Catalogs.Catalog c on p.CatalogId = c.Id
		inner join Catalogs.Assortment a on a.CatalogId = c.Id
		inner join Catalogs.Producers pr on a.ProducerId = pr.Id
		inner join Catalogs.CatalogNames cn on c.NameId = cn.Id
		inner join Catalogs.CatalogForms cf on c.FormId = cf.Id
		left join Farm.Synonym s on c0.SynonymCode = s.SynonymCode
		left join Farm.SynonymFirmCr sf on c0.SynonymFirmCrCode = sf.SynonymFirmCrCode
		left join Catalogs.Producers ppr on c0.CodeFirmCr = ppr.Id
	where p.Hidden = 0
		and a.ProducerId in ({producerIds})
		and c0.PriceCode in ({priceIds})
	group by c0.ProductId
	order by 4";
			return dbSession.Connection.Query<ProductsGridViewModel>(sql).ToList();
		}
	}

	public class PromotionFromAssortmentViewModel
	{
		public PromotionFromAssortmentViewModel()
		{
			Method = SelectMethod.SelectFromList;
			AvailableProducts = new List<ProductsGridViewModel>();
		}

		public SelectMethod Method { get; set; }
		public uint MarketingEventId { get; set; }
		public string MarketingEventName { get; set; }
		public uint PromotionId { get; set; }
		public string PromotionName { get; set; }
		public IList<Producer> Producers { get; set; }
		public string SelectedProductIds { get; set; }
		public IList<ProductsGridViewModel> AvailableProducts { get; set; }

		public void Init(ISession dbSession, uint promotionId)
		{
			var promotion = dbSession.Query<ProducerPromotion>()
				.First(r => r.Id == promotionId);
			MarketingEventId = promotion.MarketingEvent.Id;
			MarketingEventName = promotion.MarketingEvent.Name;
			PromotionId = promotionId;
			PromotionName = promotion.Name;
			Producers = promotion.MarketingEvent.Producers.Select(r => r.Producer).ToList();
			SelectedProductIds = string.Join(",", promotion.Products.Select(r => r.Product.Id.ToString()).ToArray());
			var producerIds = string.Join(",", Producers.Select(r => r.Id.ToString()).ToArray());
			AvailableProducts = GetProductsList(dbSession, producerIds);
		}

		private IList<ProductsGridViewModel> GetProductsList(ISession dbSession, string producerIds)
		{
			if (string.IsNullOrEmpty(producerIds))
				return new List<ProductsGridViewModel>();

			var sql = $@"select c0.ProductId, c0.Code, c0.CodeCr, ifnull(s.Synonym, c.Name) as ProductName,
		ifnull(sf.Synonym, pr.Name) as ProducerName, cn.Name as CatalogName, cf.Form as CatalogFormName,
		(select cast(
				group_concat(ifnull(cpv.Value, '')
					order by cp.PropertyName, cpv.Value
					separator ', ')
				as char)
			from Catalogs.ProductProperties cpp
				left join Catalogs.PropertyValues cpv on cpv.Id = cpp.PropertyValueId
				left join Catalogs.Properties cp on cp.Id = cpv.PropertyId
			where cpp.ProductId = p.Id
		) as CatalogProperty,
		ifnull(ppr.Name, '') as CatalogProducer, ifnull(ppr.Name, '') as MainCatalogProducer
	from Farm.Core0 c0
		inner join Catalogs.Products p on c0.ProductId = p.Id
		inner join Catalogs.Catalog c on p.CatalogId = c.Id
		inner join Catalogs.Assortment a on a.CatalogId = c.Id
		inner join Catalogs.Producers pr on a.ProducerId = pr.Id
		inner join Catalogs.CatalogNames cn on c.NameId = cn.Id
		inner join Catalogs.CatalogForms cf on c.FormId = cf.Id
		left join Farm.Synonym s on c0.SynonymCode = s.SynonymCode
		left join Farm.SynonymFirmCr sf on c0.SynonymFirmCrCode = sf.SynonymFirmCrCode
		left join Catalogs.Producers ppr on c0.CodeFirmCr = ppr.Id
	where p.Hidden = 0
		and a.ProducerId in ({producerIds})
	group by c0.ProductId
	order by 4";
			return dbSession.Connection.Query<ProductsGridViewModel>(sql).ToList();
		}
	}

	public enum SelectMethod : byte
	{
		[Description("Выбор из списка")] SelectFromList,
		[Description("Выбор из прайса")] SelectFromPrice
	}
}
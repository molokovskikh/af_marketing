using Dapper;
using System;
using System.Linq;
using System.Web.Mvc;
using DevExpress.Web.Mvc;
using Marketing.Models;
using Marketing.ViewModels;
using NHibernate.Linq;
using System.Collections.Generic;
using NHibernate.Util;

namespace Marketing.Controllers
{
	public class PromoterProducersItem
	{
		public virtual uint Id { get; set; }
		public virtual Producer Producer { get; set; }
		public virtual string Contacts { get; set; }
	}

	[Authorize]
	public class MarketingController : BaseController
	{
		private PromotionFromPriceViewModel _editedPromotionFromPrice;
		private PromotionFromPriceViewModel EditedPromotionFromPrice
		{
			get
			{
				if (_editedPromotionFromPrice == null)
					_editedPromotionFromPrice =
						System.Web.HttpContext.Current.Session["EditedPromotionFromPrice"] as PromotionFromPriceViewModel;
				return _editedPromotionFromPrice;
			}
			set
			{
				_editedPromotionFromPrice = null;
				System.Web.HttpContext.Current.Session["EditedPromotionFromPrice"] = value;
			}
		}

		public ActionResult Index()
		{
			var model = GetGridData();
			return View(model);
		}

		public ActionResult IndexGridView()
		{
			var model = GetGridData();
			return PartialView("partials/_IndexGridView", model);
		}

		private IList<MarketingEventGridModel> GetGridData()
		{
			return DbSession.Query<MarketingEvent>()
				.Where(s => s.Promoter == CurrentPromoter)
				.FetchMany(r => r.Producers)
				.ThenFetch(r => r.Producer)
				.OrderBy(s => s.Name)
				.ToList()
				.Select(r => new MarketingEventGridModel
				{
					MarketingEventId = r.Id,
					Name = r.Name,
					Producers = string.Join(",", r.Producers.Select(p => p.Producer.Name).OrderBy(p => p).ToArray()),
					PromotionCount = r.Promotions.Count,
					EnabledPromotionCount = r.Promotions.Count(p => p.Enabled),
					ActivePromotionCount = r.Promotions.Count(p => p.Enabled && p.DateStarted <= DateTime.Today && p.DateFinished >= DateTime.Today)
				})
				.ToList();
		}

		public ActionResult Add()
		{
			var model = new MarketingEventViewModel();
			model.AvailableProducers = DbSession.Query<Producer>()
				.OrderBy(r => r.Name)
				.ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult Add(MarketingEventViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var marketingEvent = new MarketingEvent {
				Promoter = CurrentPromoter,
				Name = model.Name
			};
			DbSession.Save(marketingEvent);

			if (!string.IsNullOrWhiteSpace(model.SelectedProducerIds)) {
				var ids = model.SelectedProducerIds.Split(',').Select(p => uint.Parse(p)).ToArray();
				ids.ForEach(p => {
					DbSession.CreateSQLQuery(
							"insert into Customers.PromoterProducers (ProducerId, MarketingEventId) values (:id, :eventId)")
						.SetParameter("id", p)
						.SetParameter("eventId", marketingEvent.Id)
						.ExecuteUpdate();
				});
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult GetProducersList()
		{
			var model = new MarketingEventViewModel {
				AvailableProducers = DbSession.Query<Producer>()
					.OrderBy(r => r.Name)
					.ToList()
			};
			return PartialView("partials/_AddGridView", model);
		}

		public ActionResult Edit(uint id)
		{
			var marketingEvent = DbSession.Query<MarketingEvent>().SingleOrDefault(r => r.Id == id);
			if (marketingEvent == null)
				return HttpNotFound();

			var model = new MarketingEventViewModel() {
				MarketingEventId = marketingEvent.Id,
				Name = marketingEvent.Name,
				SelectedProducerIds = string.Join(",", marketingEvent.Producers.Select(r => r.Producer.Id.ToString()).ToArray())
			};
			model.AvailableProducers = DbSession.Query<Producer>()
				.OrderBy(r => r.Name)
				.ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult Edit(MarketingEventViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var marketingEvent = DbSession.Query<MarketingEvent>().SingleOrDefault(r => r.Id == model.MarketingEventId);
			marketingEvent.Name = model.Name;
			DbSession.Update(marketingEvent);

			var producers = DbSession.Query<PromoterProducer>()
				.Where(r => r.MarketingEvent == marketingEvent)
				.ToArray();
			var ids = model.SelectedProducerIds.Split(',').Select(p => uint.Parse(p)).ToArray();
			producers.Where(r => !ids.Contains(r.Producer.Id)).ForEach(p => { DbSession.Delete(p); });
			ids.Where(r => !producers.Any(p => p.Producer.Id == r)).ForEach(x => {
				DbSession.CreateSQLQuery(
						"insert into Customers.PromoterProducers (ProducerId, MarketingEventId) values (:id, :eventId)")
					.SetParameter("id", x)
					.SetParameter("eventId", marketingEvent.Id)
					.ExecuteUpdate();
			});

			return RedirectToAction("Index");
		}

		public ActionResult Delete(uint id)
		{
			var marketingEvent = DbSession.Query<MarketingEvent>().SingleOrDefault(r => r.Id == id);
			if (marketingEvent == null)
				return HttpNotFound();
			DbSession.Delete(marketingEvent);
			SuccessMessage($"Маркетинговое мероприятие \"{marketingEvent.Name}\" успешно удалено");
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult GetFilterProducerAdd()
		{
			var result = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
				.Where(s => CurrentPromoter.MarketingEvents.All(f => f.Producers.All(p => p.Producer.Id != s.Id)))
				.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
				.ToList();
			return PartialView("../_default/ProducerAddFilter", result);
		}

		public ActionResult PromotionList(uint id)
		{
			System.Web.HttpContext.Current.Session["MarketingEvent"] = DbSession.Query<MarketingEvent>().Single(r => r.Id == id);
			var model = DbSession.Query<ProducerPromotion>()
				.Where(s => s.MarketingEvent == CurrentMarketingEvent)
				.OrderBy(s => s.Name)
				.ToList();

			ViewBag.MarketingEvent = CurrentMarketingEvent;
			return View(model);
		}

		public ActionResult PromotionListGridView(uint id)
		{
			return PartialView("partials/_PromotionListGridView", DbSession.Query<ProducerPromotion>()
				.Where(s => s.MarketingEvent == CurrentMarketingEvent)
				.OrderBy(s => s.Name)
				.ToList());
		}

		[HttpGet]
		public ActionResult PromotionAdd(uint id)
		{
			var model = new ProducerPromotion();
			model.MarketingEvent = CurrentMarketingEvent;
			model.Enabled = true;
			ViewBag.MarketingEvent = CurrentMarketingEvent;
			return View(model);
		}

		[HttpPost]
		public ActionResult PromotionAdd(ProducerPromotion model)
		{
			if (!this.ModelState.IsValid)
			{
				ViewBag.MarketingEvent = CurrentMarketingEvent;
				return View(model);
			}
			var newItem = new ProducerPromotion() {
				MarketingEvent = CurrentMarketingEvent,
				Name = model.Name,
				DateStarted = model.DateStarted,
				DateFinished = model.DateFinished,
				Enabled = model.Enabled
			};
			DbSession.Save(newItem);

			SuccessMessage($"Акция \"{newItem.Name}\" успешно добавлена.");

			return RedirectToAction("PromotionList", new {id = CurrentMarketingEvent.Id});
		}


		public ActionResult PromotionEditListManager(uint id, string list,
			PromotionTableRequestType type, string regionList)
		{
			if (type == PromotionTableRequestType.SuppliersListToGet || type == PromotionTableRequestType.SuppliersListToSet) {
				var model = new PromotionTableSelectorViewModel<ViewModelRegionListItem>();
				model.SetData(DbSession, id, type, list, regionList);
				return PartialView("partials/_PromotionEditListGridView", model);
			} else {
				var model = new PromotionTableSelectorViewModel<ViewModelListItem>();
				model.SetData(DbSession, id, type, list, regionList);
				return PartialView("partials/_PromotionEditListGridView", model);
			}
		}

		public ActionResult GetFilterRegion(string currentValues = "")
		{
			var model =
				DbSession.Query<Region>()
					.Where(s => s.Id != 0 && s.Name != "Inforoom" && s.DrugsSearchRegion == false)
					.OrderBy(s => s.Name)
					.Select(s => new ViewModelListItem {Text = s.Name, Value = s.Id})
					.ToList();
			return PartialView("partials/_PromotionEditRegionFilterLogic", model);
		}

		[HttpGet]
		public ActionResult PromotionEdit(uint id)
		{
			return PromotionEditFromList(id);
		}

		[HttpPost]
		public ActionResult PromotionEdit(uint id, SelectMethod method)
		{
			if (method == SelectMethod.SelectFromList)
				return PromotionEditFromList(id);
			else
				return PromotionEditFromPrice(id);
		}

		public ActionResult PromotionEditFromList(uint id)
		{
			var model = new PromotionViewModel();
			model.SetData(DbSession, id);
			return View("PromotionEdit", model);
		}

		public ActionResult PromotionEditFromPrice(uint id)
		{
			var model = new PromotionFromPriceViewModel();
			model.SetData(DbSession, id);
			EditedPromotionFromPrice = model;
			return View("PromotionEditFromPrice", model);
		}

		[HttpPost]
		public ActionResult GetSuppliersList()
		{
			return PartialView("partials/_SuppliersGrid", EditedPromotionFromPrice);
		}

		[HttpPost]
		public ActionResult GetPricesList()
		{
			return PartialView("partials/_PricesGrid", EditedPromotionFromPrice);
		}

		[HttpPost]
		public ActionResult GetProductsList()
		{
			return PartialView("partials/_GoodsGrid", EditedPromotionFromPrice);
		}

		[HttpPost]
		public ActionResult ChangeSupplierList(string selectedIds)
		{
			if (string.IsNullOrEmpty(selectedIds)) {
				EditedPromotionFromPrice.AvailablePrices = new List<PricesGridViewModel>();
				return Content("");
			}

			EditedPromotionFromPrice.SelectedSupplierIds = selectedIds;
			var producerIds = string.Join(",",
				EditedPromotionFromPrice.Producers.Select(r => r.Id.ToString()).ToArray());
			if (string.IsNullOrEmpty(producerIds))
				return HttpNotFound();

			var sql = $@"select pd.PriceCode as PriceId, pd.PriceName as Name, pi.PriceDate
	from usersettings.pricesdata pd
		inner join usersettings.pricescosts pc on pc.PriceCode = pd.pricecode
		inner join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
	where pd.Enabled = 1
		and pd.FirmCode in ({selectedIds})
		and pd.PriceCode in (
			select c0.PriceCode
				from Farm.Core0 c0
					inner join Catalogs.products p on c0.ProductId = p.Id
					inner join Catalogs.catalog c on p.CatalogId = c.Id
					inner join Catalogs.Assortment a on a.CatalogId = c.Id
				where p.Hidden = 0
					and a.ProducerId in ({producerIds})
			)";
			EditedPromotionFromPrice.AvailablePrices = DbSession.Connection.Query<PricesGridViewModel>(sql).ToList();

			return Content("");
		}

		[HttpPost]
		public ActionResult ChangePriceList(string selectedIds)
		{
			if (string.IsNullOrEmpty(selectedIds)) {
				EditedPromotionFromPrice.AvailableProducts = new List<ProductsGridViewModel>();
				return Content("");
			}

			EditedPromotionFromPrice.SelectedPriceIds = selectedIds;
			var producerIds = string.Join(",",
				EditedPromotionFromPrice.Producers.Select(r => r.Id.ToString()).ToArray());
			if (string.IsNullOrEmpty(producerIds))
				return HttpNotFound();

			var sql = $@"select distinct c0.PriceCode, c0.ProductId, a.ProducerId, c.Name as ProductName, pr.Name as ProducerName,
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
		and c0.PriceCode in ({selectedIds})";
			EditedPromotionFromPrice.AvailableProducts = DbSession.Connection.Query<ProductsGridViewModel>(sql).ToList();

			return Content("");
		}

		[HttpPost]
		public ActionResult PromotionFromPriceSave(PromotionFromPriceViewModel model)
		{
			var promotion = DbSession.Query<ProducerPromotion>().FirstOrDefault(r => r.Id == model.PromotionId);
			if (promotion == null)
				return HttpNotFound();

			var suppliers = DbSession.Query<PromotionSupplier>()
				.Where(r => r.Promotion == promotion)
				.ToArray();
			var supplierIds = model.SelectedSupplierIds.Split(',').Select(r => uint.Parse(r)).ToArray();
			suppliers.Where(r => !supplierIds.Contains(r.Supplier.Id)).ForEach(r => DbSession.Delete(r));
			supplierIds.Where(s => !suppliers.Any(r => r.Supplier.Id == s)).ForEach(r => {
				var supplier = new PromotionSupplier {
					Promotion = promotion,
					Supplier = DbSession.Query<Supplier>().First(s => s.Id == r)
				};
				DbSession.Save(supplier);
			});

			var products = DbSession.Query<PromotionProduct>()
				.Where(r => r.Promotion == promotion)
				.ToArray();
			var productIds = model.SelectedProductIds.Split(',').Select(r => uint.Parse(r)).ToArray();
			products.Where(r => !productIds.Contains(r.Product.Id)).ForEach(r => DbSession.Delete(r));
			productIds.Where(p => !products.Any(r => r.Product.Id == p)).ForEach(r => {
				var product = new PromotionProduct {
					Promotion = promotion,
					Product = DbSession.Query<Product>().First(p => p.Id == r)
				};
				DbSession.Save(product);
			});

			return RedirectToAction("PromotionList", new {id = CurrentMarketingEvent.Id});
		}

		[HttpGet]
		public ActionResult PromotionChange(uint id)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == id);
			ViewBag.MarketingEvent = CurrentMarketingEvent;
			return View(promotion);
		}

		[HttpPost]
		public ActionResult PromotionChange(ProducerPromotion model)
		{
			if (!ModelState.IsValid) {
				ViewBag.MarketingEvent = CurrentMarketingEvent;
				return View(model);
			}
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == model.Id);
			promotion.Name = model.Name;
			promotion.DateStarted = model.DateStarted;
			promotion.DateFinished = model.DateFinished;
			promotion.Enabled = model.Enabled;
			DbSession.Save(promotion);
			return RedirectToAction("PromotionList", new {id = CurrentMarketingEvent.Id});
		}


		public ActionResult PromotionDelete(uint id)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == id);
			DbSession.Delete(promotion);
			SuccessMessage($"Акция \"{promotion.Name}\" успешно удалена.");
			return RedirectToAction("PromotionList", new { id = CurrentMarketingEvent.Id });
		}


		public ActionResult PromotionEditGet(PromotionViewModel model)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == model.Promotion.Id);

			promotion.UpdateProductsAndSuppliersByIds(DbSession, model.ProductsListToSetList, model.SuppliersListToSetList);

			return RedirectToAction("PromotionList",new {id = promotion.MarketingEvent.Id});
		}

		[HttpGet]
		public ActionResult PromotionСonditionsEdit(uint id)
		{
			var model = GetConditionsViewModel(id);
			return View(model);
		}

		public ActionResult GetConditionsGridData(uint id)
		{
			var model = GetConditionsViewModel(id);
			return PartialView("partials/_ConditionsGridView", model);
		}

		[ValidateInput(false)]
		public ActionResult EditConditionsBatch(uint id,
			MVCxGridViewBatchUpdateValues<ConditionsGridViewModel, int> updateValues)
		{
			foreach (var item in updateValues.Update) {
				if (updateValues.IsValid(item))
					try {
						var condition = DbSession.Query<PromotionProduct>().Single(r => r.Id == item.ConditionId);
						condition.Price = item.Price;
						condition.DealerPercent = item.DealerPercent;
						condition.MemberPercent = item.MemberPercent;
					} catch (Exception ex) {
						updateValues.SetErrorText(item, ex.Message);
					}
			}
			var model = GetConditionsViewModel(id);
			return PartialView("partials/_ConditionsGridView", model);
		}

		private PromotionConditionsViewModel GetConditionsViewModel(uint id)
		{
			var promotion = DbSession.Query<ProducerPromotion>().FirstOrDefault(r => r.Id == id);
			var query = DbSession.Query<PromotionProduct>()
				.Where(r => r.Promotion == promotion)
				.Fetch(r => r.Product)
				.ThenFetch(r => r.Catalog);
			var conditions = query
				.Select(r => new ConditionsGridViewModel {
					PromotionId = promotion.Id,
					ConditionId = r.Id,
					ProductName = r.Product.Catalog.Name,
					Price = r.Price,
					DealerPercent = r.DealerPercent,
					MemberPercent = r.MemberPercent
				})
				.ToList();
			var model = new PromotionConditionsViewModel {
				Conditions = conditions,
				Promotion = promotion,
				MarketingEvent = CurrentMarketingEvent
			};
			return model;
		}

		[HttpPost]
		public ActionResult PromotionСonditionsEdit(uint id, string contacts)
		{
			return RedirectToAction("PromotionList", new { id = CurrentMarketingEvent.Id });
		}
	}
}
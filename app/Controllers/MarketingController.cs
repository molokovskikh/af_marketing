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
		private MarketingEventViewModel _editedEvent;
		private MarketingEventViewModel EditedEvent
		{
			get
			{
				if (_editedEvent == null)
					_editedEvent = System.Web.HttpContext.Current.Session["editedEvent"] as MarketingEventViewModel;
				return _editedEvent;
			}
			set
			{
				_editedEvent = null;
				System.Web.HttpContext.Current.Session["editedEvent"] = value;
			}
		}

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
				.Where(s => s.Association == CurrentAssociation)
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
			model.AddMode = true;
			model.AvailableProducers = DbSession.Query<Producer>()
				.OrderBy(r => r.Name)
				.ToList();
			EditedEvent = model;
			return View("Edit", model);
		}

		[HttpPost]
		public ActionResult Save(MarketingEventViewModel model)
		{
			if (!ModelState.IsValid)
				return View("Edit", model);

			if (model.AddMode) {
				var marketingEvent = new MarketingEvent {
					Association = CurrentAssociation,
					Name = model.Name
				};
				DbSession.Save(marketingEvent);

				EditedEvent.SelectedProducers.ForEach(p => {
					DbSession.CreateSQLQuery(
							"insert into Customers.PromoterProducers (ProducerId, MarketingEventId) values (:id, :eventId)")
						.SetParameter("id", p.Id)
						.SetParameter("eventId", marketingEvent.Id)
						.ExecuteUpdate();
				});
			}
			else {
				var marketingEvent = DbSession.Query<MarketingEvent>().FirstOrDefault(r => r.Id == model.MarketingEventId);
				if (marketingEvent == null)
					return HttpNotFound();

				var producers = DbSession.Query<PromoterProducer>()
					.Where(r => r.MarketingEvent == marketingEvent)
					.ToArray();
				producers.Where(r => !EditedEvent.SelectedProducers.Contains(r.Producer)).ForEach(p => { DbSession.Delete(p); });
				EditedEvent.SelectedProducers.Where(r => !producers.Any(p => p.Producer == r)).ForEach(x => {
					DbSession.CreateSQLQuery(
							"insert into Customers.PromoterProducers (ProducerId, MarketingEventId) values (:id, :eventId)")
						.SetParameter("id", x.Id)
						.SetParameter("eventId", marketingEvent.Id)
						.ExecuteUpdate();
				});
			}

			EditedEvent = null;

			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult GetAvailableProducersList()
		{
			return PartialView("partials/_SelectingProducersGrid", EditedEvent);
		}

		[HttpPost]
		public ActionResult GetSelectedProducersList()
		{
			return PartialView("partials/_SelectedProducersGrid", EditedEvent);
		}

		[HttpPost]
		public string AddProducers(string selectedIds)
		{
			var ids = selectedIds.Split(',').Select(r => uint.Parse(r)).ToArray();
			ids.ForEach(r => {
				var item = DbSession.Query<Producer>().FirstOrDefault(p => p.Id == r);
				if (item != null)
					EditedEvent.SelectedProducers.Add(item);
			});
			EditedEvent.SelectedProducers = EditedEvent.SelectedProducers.OrderBy(r => r.Name).ToList();
			EditedEvent.AvailableProducers = DbSession.Query<Producer>()
				.ToList()
				.Where(r => !EditedEvent.SelectedProducers.Contains(r))
				.ToList();
			return "";
		}

		[HttpPost]
		public string RemoveProducers(string selectedIds)
		{
			var ids = selectedIds.Split(',').Select(r => uint.Parse(r)).ToArray();
			ids.ForEach(r => {
				var item = EditedEvent.SelectedProducers.FirstOrDefault(p => p.Id == r);
				if (item != null)
					EditedEvent.SelectedProducers.Remove(item);
			});
			EditedEvent.AvailableProducers = DbSession.Query<Producer>()
				.ToList()
				.Where(r => !EditedEvent.SelectedProducers.Contains(r))
				.ToList();
			return "";
		}

		public ActionResult Edit(uint id)
		{
			var marketingEvent = DbSession.Query<MarketingEvent>().SingleOrDefault(r => r.Id == id);
			if (marketingEvent == null)
				return HttpNotFound();

			var model = new MarketingEventViewModel() {
				MarketingEventId = marketingEvent.Id,
				Name = marketingEvent.Name,
			};
			model.SelectedProducers = DbSession.Query<Producer>()
				.ToList()
				.Where(r => marketingEvent.Producers.Any(p => p.Producer == r))
				.OrderBy(r => r.Name)
				.ToList();
			model.AvailableProducers = DbSession.Query<Producer>()
				.Where(r => !model.SelectedProducers.Contains(r))
				.OrderBy(r => r.Name)
				.ToList();
			EditedEvent = model;

			return View(model);
		}

		[HttpPost]
		public ActionResult SaveName(uint id, string name)
		{
			var marketingEvent = DbSession.Query<MarketingEvent>().FirstOrDefault(r => r.Id == id);
			if (marketingEvent == null)
				return HttpNotFound();
			marketingEvent.Name = name;
			DbSession.Update(marketingEvent);
			EditedEvent.Name = name;
			return Content("");
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
				.Where(s => CurrentAssociation.MarketingEvents.All(f => f.Producers.All(p => p.Producer.Id != s.Id)))
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
			model.DateStarted = DateTime.Today.AddMonths(1);
			model.DateStarted = model.DateStarted.AddDays(1 - model.DateStarted.Day);
			model.DateFinished = model.DateStarted.AddYears(1);
			model.DateFinished =
				model.DateFinished.AddDays(1 - model.DateFinished.Day).AddMonths(1 - model.DateFinished.Month).AddDays(-1);
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
			model.Init(DbSession, id);
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

			EditedPromotionFromPrice.AvailablePrices = EditedPromotionFromPrice.GetPricesList(DbSession, producerIds, selectedIds);

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

			EditedPromotionFromPrice.AvailableProducts = EditedPromotionFromPrice.GetProductsList(DbSession, producerIds, selectedIds);

			var countNoProducer = EditedPromotionFromPrice.AvailableProducts.Count(r => string.IsNullOrEmpty(r.CatalogProducer));

			return Json(new { countNoProducer = countNoProducer });
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

			EditedPromotionFromPrice = null;

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
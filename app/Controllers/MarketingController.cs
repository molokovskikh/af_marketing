using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Marketing.Helpers;
using Marketing.Models;
using Marketing.ViewModels;
using NHibernate;
using NHibernate.Linq;
using DevExpress.Web.Mvc;
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
		[ValidateAntiForgeryToken]
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
				SelectedProducerIds = string.Join(",", marketingEvent.Producers.Select(r => r.Id.ToString()).ToArray())
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

			//if (!string.IsNullOrWhiteSpace(model.SelectedProducerIds))
			//{
			//	var ids = model.SelectedProducerIds.Split(',').Select(p => uint.Parse(p)).ToArray();
			//	ids.ForEach(p => {
			//		DbSession.CreateSQLQuery(
			//				"insert into Customers.PromoterProducers (ProducerId, MarketingEventId) values (:id, :eventId)")
			//			.SetParameter("id", p)
			//			.SetParameter("eventId", marketingEvent.Id)
			//			.ExecuteUpdate();
			//	});
			//}

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

		//[HttpGet]
		//public ActionResult ProducerAdd()
		//{
		//	var model = new PromoterProducersViewModel();
		//	model.ProducersList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
		//		.Where(s => CurrentPromoter.MarketingEvents.First().Producers.All(f => f.Producer.Id != s.Id))
		//		.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
		//	return View(model);
		//}

		//[HttpPost]
		//public ActionResult ProducerAdd(PromoterProducersViewModel model)
		//{
		//	if (!this.ModelState.IsValid) {
		//		model.ProducersList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
		//			.Where(s => CurrentPromoter.MarketingEvents.All(f => f.Producer.Id != s.Id))
		//			.Select(s => new SelectListItem {Value = s.Id.ToString(), Text = s.Name})
		//			.ToList();
		//		return View(model);
		//	}
		//	var currentProducer = DbSession.Query<Producer>().First(s => s.Id == model.SelectedProducerId);
		//	if (CurrentPromoter.MarketingEvents.Any(s => s.Producer.Id == currentProducer.Id)) {
		//		ErrorMessage($"Поставщик \"{currentProducer.Name}\" не может быть добавлен повторно.");
		//	} else {
		//		var newItem = new PromoterProducer {
		//			MarketingEvent = CurrentPromoter,
		//			Producer = currentProducer,
		//			Contacts = model.Contacts
		//		};
		//		DbSession.Save(newItem);

		//		SuccessMessage($"Поставщик \"{newItem.Producer.Name}\" успешно добавлен.");
		//	}

		//	return RedirectToAction("Index");
		//}

		//[HttpPost]
		//public ActionResult GetFilterProducerAdd()
		//{
		//	var result = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
		//		//.Where(s => CurrentPromoter.MarketingEvents.All(f => f.Producer.Id != s.Id))
		//		.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
		//		.ToList();
		//	return PartialView("../_default/ProducerAddFilter", result);
		//}

		//[HttpGet]
		//public ActionResult ProducerEdit(uint id)
		//{
		//	var model = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
		//	return View(model);
		//}

		//[HttpPost]
		//public ActionResult ProducerEdit(uint id, string contacts)
		//{
		//	var model = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
		//	model.Contacts = contacts;
		//	SuccessMessage($"Контакты поставщика \"{model.Producer.Name}\" успешно изменен.");
		//	return RedirectToAction("Index");
		//}
		//public ActionResult ProducerDelete(uint id)
		//{
		//	var model = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
		//	DbSession.Delete(model);
		//	SuccessMessage($"Поставщик \"{model.Producer.Name}\" успешно удален.");
		//	return RedirectToAction("Index");
		//}

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
			PromotionTableSelectorViewModel.RequestType type, string regionList)
		{
			var model = new PromotionTableSelectorViewModel();
			model.SetData(DbSession, id, type, list, regionList);
			return PartialView("partials/_PromotionEditListGridView", model);
		}
		public ActionResult GetFilterRegion(string currentValues = "")
		{
			var model =
				DbSession.Query<Region>()
					.Where(s => s.Id != 0)
					.OrderBy(s => s.Name)
					.Select(s => new SelectListItem {Text = s.Name, Value = s.Id.ToString()})
					.ToList();
		//	model.Insert(0,new SelectListItem() {Value = "0",Text = "Все регионы"});
			return PartialView("partials/_PromotionEditRegionFilterLogic", model);
		}

		[HttpGet]
		public ActionResult PromotionEdit(uint id)
		{
			var model = new PromotionViewModel();
			model.SetData(DbSession, id);
			return View(model);
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


		public ActionResult PromotionrEditGet(PromotionViewModel model)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == model.Promotion.Id);

			promotion.UpdateProductsAndSuppliersByIds(DbSession, model.ProductsListToSetList, model.SuppliersListToSetList);

			return RedirectToAction("PromotionList",new {id = CurrentMarketingEvent.Id});
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
		public ActionResult EditConditionsBatch(uint id, MVCxGridViewBatchUpdateValues<ConditionsGridViewModel, int> updateValues)
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
				.Select(r => new ConditionsGridViewModel
				{
					PromotionId = promotion.Id,
					ConditionId = r.Id,
					ProductName = r.Product.Catalog.Name,
					Price = r.Price,
					DealerPercent = r.DealerPercent,
					MemberPercent = r.MemberPercent
				})
				.ToList();
			var model = new PromotionConditionsViewModel
			{
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
			//return RedirectToAction("Index");
		}
	}
}
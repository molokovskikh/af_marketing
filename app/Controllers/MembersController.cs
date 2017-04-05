using Marketing.Models;
using Marketing.ViewModels;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marketing.Controllers
{
#if !DEBUG
	//[Authorize]
#endif
	public class MembersController : BaseController
	{
		public ActionResult Index()
		{
			var model = GetMemberList();
			return View(model);
		}

		public ActionResult GetGridData()
		{
			var model = GetMemberList();
			return PartialView("GridView", model);
		}

		private List<MembersGridViewModel> GetMemberList()
		{
			var result = DbSession.Query<PromotionMember>()
				.Where(r => r.Promoter == CurrentPromoter)
				.ToList()
				.Select(r => new MembersGridViewModel {
					MemberId = r.Id,
					Name = r.Client.Name,
					AddressCount = r.Client.Addresses.Count,
					Subscribes = string.Join(",", r.Subscribes.Select(s => s.Promotion.Name).ToArray())
				})
				.OrderBy(r => r.Name);
			return result.ToList();
		}

		public ActionResult Add()
		{
			var model = new MemberViewModel();
			var exist = DbSession.Query<PromotionMember>()
				.Where(r => r.Promoter == CurrentPromoter)
				.Select(r => r.Client)
				.ToList();
			model.AvailableMembers = DbSession.Query<Client>()
				.Where(r => !exist.Contains(r))
				.OrderBy(r => r.Name)
				.Select(r => new SelectListItem {
					Value = r.Id.ToString(),
					Text = r.Name
				})
				.ToList();
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Add(MemberViewModel model)
		{
			if (!ModelState.IsValid)
				return RedirectToAction("Add");

			var client = DbSession.Query<Client>().FirstOrDefault(r => r.Id == model.MemberId);
			if (client == null)
				return HttpNotFound("Выбранный клиент не найден.");

			var member = new PromotionMember {
				Promoter = CurrentPromoter,
				Client = client
			};
			DbSession.Save(member);
			DbSession.Flush();

			return RedirectToAction("Index");
		}

		public ActionResult Edit(uint id)
		{
			var client = DbSession.Query<PromotionMember>()
				.FirstOrDefault(r => r.Id == id)
				.Client;
			var model = new ClientViewModel {
				Name = client.Name,
				Addresses = client.Addresses
			};
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(ClientViewModel model)
		{
			return RedirectToAction("Index");
		}

		public ActionResult Subscribes(uint id)
		{
			var model = GetSubscribesModel(id);
			return View(model);
		}

		public ActionResult SubscribesList(uint memberId, uint[] selectedPromotions)
		{
			var member = DbSession.Query<PromotionMember>().Single(r => r.Id == memberId);
			var subscribes = DbSession.Query<PromotionSubscribe>()
				.Where(r => r.Member == member)
				.ToList();

			var del = subscribes
				.Where(r => !selectedPromotions.Any(p => p == r.Promotion.Id))
				.ToList();
			del.ForEach(r => DbSession.Delete(r));
			DbSession.Flush();

			foreach (var id in selectedPromotions) {
				if (!subscribes.Any(r => r.Promotion.Id == id)) {
					var subscribe = new PromotionSubscribe {
						Member = member,
						Promotion = DbSession.Query<ProducerPromotion>().Single(r => r.Id == id)
					};
					DbSession.Save(subscribe);
				}
			}
			DbSession.Flush();

			var model = GetSubscribesModel(memberId);
			return PartialView(model);
		}

		private MemberSubscribesViewModel GetSubscribesModel(uint id)
		{
			var member = DbSession.Query<PromotionMember>().Single(r => r.Id == id);
			var promoter = member.Promoter;
			var subscribes = DbSession.Query<PromotionSubscribe>()
				.Where(r => r.Member == member)
				.ToList();
			var promotions = DbSession.Query<ProducerPromotion>()
				.Where(r => r.Producer.Promoter == promoter)
				.ToList()
				.Select(r => new MemberSubscribe {
					PromotionId = r.Id,
					PromotionName = subscribes.Any(s => s.Promotion.Id == r.Id) ? "*" + r.Name : r.Name,
				})
				.ToList();
			var model = new MemberSubscribesViewModel {
				MemberId = member.Id,
				MemberName = member.Client.Name,
				Promotions = promotions
			};
			return model;
		}
	}
}
using Marketing.Models;
using Marketing.ViewModels;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Marketing.Controllers
{
#if !DEBUG
	[Authorize]
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
				.Select(r => new MembersGridViewModel {
					MemberId = r.Id,
					Name = r.Client.Name,
					AddressCount = r.Client.Addresses.Count,
					Subscribes = ""
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
	}
}
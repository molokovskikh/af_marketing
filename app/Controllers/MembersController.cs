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
	[Authorize]
	public class MembersController : BaseController
	{
		public ActionResult Index()
		{
			var model = GetMemberList();
			ViewBag.AvailableRegions = SearchRegions("");
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(string regionIdList)
		{
			var model = GetMemberList(regionIdList);
			ViewBag.AvailableRegions = SearchRegions("", regionIdList);
			return View(model);
		}

		public ActionResult GetGridData(string regionIdList = "")
		{
			var model = GetMemberList(regionIdList);
			ViewBag.AvailableRegions = SearchRegions("", regionIdList);
			return PartialView("GridView", model);
		}

		public ActionResult AvailableMembers(string regionIdList = "")
		{
			var model = GetClientList(regionIdList);
			ViewBag.AvailableRegions = SearchRegions("", regionIdList);
			return PartialView("_MembersList", model);
		}

		private List<MembersGridViewModel> GetMemberList(string idList = "")
		{
			var list = DbSession.Query<PromotionMember>()
				.Where(r => r.Promoter == CurrentPromoter)
				.ToList();
			if (!string.IsNullOrWhiteSpace(idList)) {
				var ids = idList.Split(',').Select(i => ulong.Parse(i)).ToList();
				list = list.Where(r => ids.Any(i => i == r.Client.Region.Id)).ToList();
			}
			var result = list
				.Select(r => new MembersGridViewModel {
					MemberId = r.Id,
					Name = r.Client.Name,
					Region = r.Client.Region.Name,
					AddressCount = r.Client.Addresses.Count,
					Subscribes = string.Join(", ", r.Subscribes.Select(s => s.Promotion.Name).ToArray()),
					Contacts = r.Client.ContactGroups.Any()
						? string.Join(", ", r.Client.ContactGroups.First(g => g.ContactGroupTypeId == ContactGroupType.Marketing).Contacts.OrderBy(o => o.ContactType).Select(c => c.ContactText).ToArray())
						: ""
				})
				.OrderBy(r => r.Name);
			return result.ToList();
		}

		public ActionResult Add()
		{
			var model = new MemberViewModel();
			model.AvailableMembers = GetClientList("");
			ViewBag.AvailableRegions = SearchRegions("");
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult FilterAdd(MemberViewModel model, string regionIdList = "")
		{
			model.AvailableMembers = GetClientList(regionIdList);
			ViewBag.AvailableRegions = SearchRegions("", regionIdList);
			return View("Add", model);
		}

		private List<MemberListViewModel> GetClientList(string regionIdList)
		{
			var exist = DbSession.Query<PromotionMember>()
				.Where(r => r.Promoter == CurrentPromoter)
				.Select(r => r.Client)
				.ToList();
			var clients = DbSession.Query<Client>()
				.Where(r => r.Status)
				.Where(r => !exist.Contains(r));
			if (!string.IsNullOrEmpty(regionIdList)) {
				var regionIds = regionIdList.Split(',').Select(r => ulong.Parse(r)).ToList();
				clients = clients.Where(r => regionIds.Contains(r.Region.Id));
			}
			var result = clients
				.OrderBy(r => r.Name)
				//.Select(r => new SelectListItem
				//{
				//	Value = r.Id.ToString(),
				//	Text = r.Name
				//})
				.Select(r => new MemberListViewModel
				{
					ClientId = r.Id,
					Name = r.Name,
					RegionName = r.Region.Name
				})
				.ToList();
			//result.Insert(0, new SelectListItem() { Value = "", Text = ""});
			return result;
		}

		public ActionResult GetTrailInfo(uint id)
		{
			var client = DbSession.Query<Client>()
				.FirstOrDefault(r => r.Id == id);
			if (client == null)
				return HttpNotFound();

			var model = new TrailInfoViewModel() {
				RegionName = client.Region.Name,
				Email = client.ContactGroups
					.FirstOrDefault(r => r.ContactGroupTypeId == ContactGroupType.Marketing)?
					.Contacts.FirstOrDefault(r => r.ContactType == ContactType.Email)?.ContactText,
				Phone = client.ContactGroups
					.FirstOrDefault(r => r.ContactGroupTypeId == ContactGroupType.Marketing)?
					.Contacts.FirstOrDefault(r => r.ContactType == ContactType.Phone)?.ContactText
			};
			return PartialView("_TrailInfo", model);
		}

		public ActionResult Edit(uint id)
		{
			var client = DbSession.Query<PromotionMember>()
				.FirstOrDefault(r => r.Id == id)
				.Client;
			var model = new ClientViewModel {
				ClientId = client.Id,
				Name = client.Name,
				RegionName = client.Region.Name,
				Addresses = client.Addresses,
				Email = client.ContactGroups.Any()
					? client.ContactGroups.First(r => r.ContactGroupTypeId == ContactGroupType.Marketing).Contacts.FirstOrDefault(r => r.ContactType == ContactType.Email)?.ContactText
					: null,
				Phone = client.ContactGroups.Any()
					? client.ContactGroups.First(r => r.ContactGroupTypeId == ContactGroupType.Marketing).Contacts.FirstOrDefault(r => r.ContactType == ContactType.Phone)?.ContactText
					: null
			};
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(ClientViewModel model)
		{
			var client = DbSession.Query<Client>().FirstOrDefault(r => r.Id == model.ClientId);
			if (client == null) {
				ModelState.AddModelError("", $"Клиент с идентификатором {model.ClientId} не найден.");
				return View(model);
			}

			uint ownerId;
			var contactGroup = client.ContactGroups.FirstOrDefault(r => r.ContactGroupTypeId == ContactGroupType.Marketing);
			if (contactGroup == null) {
				var owner = new ContactOwner();
				DbSession.Save(owner);
				ownerId = owner.Id;
				DbSession.CreateSQLQuery(@"
insert into Contacts.Contact_groups (Id, Name, `Type`, ContactGroupOwnerId)
	values (:ownerId, 'Маркетинг', :groupType, :groupOwnerId);")
					.SetParameter("ownerId", owner.Id)
					.SetParameter("groupType", ContactGroupType.Marketing)
					.SetParameter("groupOwnerId", client.ContactGroupOwnerId)
					.ExecuteUpdate();
			} else
				ownerId = contactGroup.Id;

			var email = DbSession.Query<Contact>().FirstOrDefault(r => r.ContactOwnerId == ownerId && r.ContactType == ContactType.Email);
			if (string.IsNullOrWhiteSpace(model.Email)) {
				if (email != null)
					DbSession.Delete(email);
			} else {
				if (email == null) {
					email = new Contact {
						ContactType = ContactType.Email,
						ContactOwnerId = ownerId
					};
				}
				email.ContactText = model.Email;
				DbSession.SaveOrUpdate(email);
			}

			var phone = DbSession.Query<Contact>().FirstOrDefault(r => r.ContactOwnerId == ownerId && r.ContactType == ContactType.Phone);
			if (string.IsNullOrWhiteSpace(model.Phone)) {
				if (phone != null)
					DbSession.Delete(phone);
			} else {
				if (phone == null) {
					phone = new Contact {
						ContactType = ContactType.Phone,
						ContactOwnerId = ownerId
					};
				}
				phone.ContactText = model.Phone;
				DbSession.SaveOrUpdate(phone);
			}

			return RedirectToAction("Index");
		}

		[ValidateInput(false)]
		public ActionResult Delete(uint memberId, string regionIdList = "")
		{
			if (memberId > 0)
				try {
					var member = DbSession.Query<PromotionMember>().Single(r => r.Id == memberId);
					DbSession.Delete(member);
					DbSession.Flush();
					SuccessMessage($"Участник \"{member.Client.Name}\" успешно удален.");
				}
				catch (Exception ex) {
					ViewData["ErrorMessage"] = ex.Message;
				}
			var model = GetMemberList(regionIdList);
			ViewBag.AvailableRegions = SearchRegions("", regionIdList);
			return View("Index", model);
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

		[HttpPost]
		public ActionResult GetFilterRegions(string term, string currentValues = "")
		{
			var model = SearchRegions(term, currentValues);
			return PartialView("../_default/RegionsFilterLogic", model);
		}

		private List<ViewModelList> SearchRegions(string term, string currentValues = "")
		{
			term = string.IsNullOrEmpty((term ?? "").Trim()) ? "%" : term;
			currentValues = string.IsNullOrEmpty(currentValues) ? "0" : currentValues;
			var itemsIdList = currentValues.Split(',').Select(s => ulong.Parse(s)).ToList();
			var result = DbSession.Query<Region>()
				.Where(r => r.Id > 0)
				.ToList()
				.Select(r => new ViewModelList {
					Value = r.Id,
					Text = r.Name,
					Selected = itemsIdList.Any(f => f == r.Id)
				}).ToList();
			return result;
		}
	}
}
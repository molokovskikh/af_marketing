using Marketing.Models;
using Marketing.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Marketing.Controllers
{
#if !DEBUG
	[Authorize]
#endif
	public class AccountController : BaseController
	{
		public const string ACC_LOGIN_PREFIX = "Marketing_";

		[AllowAnonymous]
		public ActionResult Login()
		{
			ViewBag.ReturnUrl = Request.QueryString["returnUrl"] ?? "/";
			return View(new LoginViewModel());
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel model, string returnUrl)
		{
			if (!ModelState.IsValid)
				return View(model);

			var login = ACC_LOGIN_PREFIX + model.Login;
			var skipValidation = false;
#if DEBUG
			skipValidation = !String.IsNullOrEmpty(ConfigurationManager.AppSettings["SkipLogonAD"]);
#endif
			if (!skipValidation) {
				if (!Membership.ValidateUser(login, model.Password)) {
					login = model.Login;
					if (!Membership.ValidateUser(login, model.Password)) {
						ModelState.AddModelError("", "Неверный логин/пароль.");
						return View(model);
					}
				}
			}

			var user = DbSession.QueryOver<Promoter>().Where(r => r.Login == model.Login).List().FirstOrDefault();
			if (user == null) {
				var admin = DbSession.QueryOver<RegionalAdmin>().Where(r => r.Name == model.Login).List().Count > 0;
				if (admin) {
					FormsAuthentication.SetAuthCookie(model.Login, model.RememberMe);
					return RedirectToAction("Register");
				} else {
					ModelState.AddModelError("",
						$"Пользователь \"{model.Login}\" не включен в список организаторов акций. Обратитесь в АналитФармацию.");
					return View(model);
				}
			}

			FormsAuthentication.SetAuthCookie(login, model.RememberMe);
			System.Web.HttpContext.Current.Session["promoter"] = user;

			return Redirect(returnUrl ?? "/");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			DbSession.Clear();
			return RedirectToAction("Login");
		}

		public ActionResult Register()
		{
			return View(new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			try {
				var promoter = new Promoter {
					Name = model.Name,
					Login = ""
				};
				DbSession.Save(promoter);
				promoter.Login = promoter.Id.ToString();
				DbSession.Flush();
				model.Login = promoter.Login;

				model.Password = GeneratePassword();
				//var user = Membership.CreateUser(ACC_LOGIN_PREFIX + promoter.Login, model.Password);
				//user.IsApproved = true;
				//Membership.UpdateUser(user);
			}
			catch (Exception ex) {
				DbSession.Transaction.Rollback();
				Membership.DeleteUser(model.Login);
				ModelState.AddModelError("", ex);
				return View(model);
			}

			return View("Confirm", model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Confirm(bool sendEmail)
		{
			return RedirectToAction("Register");
		}

		private string GeneratePassword()
		{
			var availableChars = "23456789qwertyupasdfghjkzxcvbnmQWERTYUPASDFGHJKLZXCVBNM";
			var password = String.Empty;
			var random = new Random();
			while (password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}
	}
}
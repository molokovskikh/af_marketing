using Marketing.Controllers;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;
using System.Web.Mvc;
using System.Linq;

namespace Marketing.Helpers
{
	public class AssociationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var controller = context.Controller as BaseController;
			if (controller != null) {
				var association = controller.CurrentAssociation;
				if (association == null) {
					var promoter = controller.CurrentPromoter;
					if (promoter != null && promoter.LastAssociationId.HasValue) {
						var dbSession = (ISession) context.HttpContext.Items[typeof(ISession)];
						association = dbSession.Query<Association>().FirstOrDefault(r => r.Id == promoter.LastAssociationId);
						NHibernateUtil.Initialize(association);
						dbSession.Evict(association);
						controller.CurrentAssociation = association;
					}
				}
				controller.ViewBag.Association = association;
			}
		}
	}
}
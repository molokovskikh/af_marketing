using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc.Html;

namespace System.Web.Mvc.Html
{
	public static partial class SelectExtensions
	{
		public static MvcHtmlString ExtEnumDropDownListFor<TModel, TEnum>(
			this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
		{
			var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
			var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

			var items = values.Select(value => new SelectListItem {
				Text = GetEnumDescription(value),
				Value = Convert.ToInt32(value).ToString(),
				Selected = value.Equals(metadata.Model)
			});
			var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
			return htmlHelper.DropDownListFor(expression, items, attributes);
		}

		public static string GetEnumDescription<TEnum>(TEnum value)
		{
			var field = value.GetType().GetField(value.ToString());
			var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Length > 0 ? attributes[0].Description : value.ToString();
		}
	}
}
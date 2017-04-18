using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace Marketing.Helpers
{
	public class MailHelper
	{
		public static void SendMail(string subject, string body, string to, string from = "")
		{
			var defSender = "tech@analit.net";
			using (var message = new MailMessage {
				From = new MailAddress(string.IsNullOrWhiteSpace(from) ? defSender : from),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			}) {
				var recipientList = to.Split(';').ToArray();
				foreach (var recipient in recipientList) {
					message.To.Add(new MailAddress(recipient));
				}

#if !DEBUG
				var server = ConfigurationManager.AppSettings["SmtpServer"];
				using (var client = new SmtpClient(server)) {
					client.Send(message);
				}
#endif
			}
		}
	}
}
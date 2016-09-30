using System;

namespace ProducerInterfaceCommon.Models
{
	public class MailFormWithFooter
	{
		public virtual int Id { get; set; }
		public virtual string Subject { get; set; }
		public virtual string Body { get; set; }
		public virtual string Footer { get; set; }
		public virtual bool IsBodyHtml { get; set; }
		public virtual string Description { get; set; }
		public virtual string Header { get; set; }
	}
}
using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;
using ProducerInterfaceCommon.ContextModels;

namespace ProducerInterfaceCommon.Models
{
	public class MediaFiles
	{
		public virtual int Id { get; set; }
		public virtual string ImageName { get; set; }
		public virtual byte[] ImageFile { get; set; }
		public virtual string ImageType { get; set; }
		public virtual uint ImageSize { get; set; }
		public virtual EntityType EntityType { get; set; }
	}
}
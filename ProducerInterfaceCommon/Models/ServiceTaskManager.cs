using System;
using System.Linq;
using Common.Tools;
using NHibernate;
using NHibernate.Linq;

namespace ProducerInterfaceCommon.Models
{
	public class ServiceTaskManager
	{
		public virtual uint Id { get; set; }
		public virtual string JobName { get; set; }
		public virtual string ServiceName { get; set; }
		public virtual string ServiceType { get; set; }
		public virtual DateTime CreationDate { get; set; }
		public virtual DateTime LastModified { get; set; }
		public virtual DateTime? LastRun { get; set; }
		public virtual User User { get; set; }
		public virtual bool Enabled { get; set; }

		public virtual string JsonData { get; set; }

		public virtual T DataFromJsonGet<T>() where T : class
		{
			try {
				return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(JsonData);
			} catch (Exception) {
				return null;
			}
		}

		public virtual void DataFromJsonSet<T>(T obj)
		{
			JsonData = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
		}
	}
}
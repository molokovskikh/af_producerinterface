using Quartz.Job.Models;
using System.ComponentModel.DataAnnotations;

namespace Quartz.Job.EDM
{
	[MetadataType(typeof(JobExtendMetaData))]
	public partial class jobextend
	{
		public Reports ReportTypeEnum
		{
			get { return (Reports)ReportType; }
		}
	}

	public class JobExtendMetaData
	{
		[ScaffoldColumn(false)]
		public string SchedName { get; set; }

		[ScaffoldColumn(false)]
		public string JobName { get; set; }

		[ScaffoldColumn(false)]
		public string JobGroup { get; set; }

		[Display(Name = "Название")]
		public string CustomName { get; set; }

		[Display(Name = "Расписание")]
		public string Scheduler { get; set; }

		[ScaffoldColumn(false)]
		public int ReportType { get; set; }

		[Display(Name = "Тип")]
		public Reports ReportTypeEnum { get; }

		[ScaffoldColumn(false)]
		public long ProducerId { get; set; }

		[Display(Name = "Создатель")]
		public string Creator { get; set; }

		[ScaffoldColumn(false)]
		public System.DateTime CreationDate { get; set; }

		[Display(Name = "Последние изменения")]
		public System.DateTime LastModified { get; set; }

		[ScaffoldColumn(false)]
		public bool Enable { get; set; }
	}
}
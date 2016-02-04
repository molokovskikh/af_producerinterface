using ProducerInterfaceCommon.ContextModels;
using ProducerInterfaceCommon.Heap;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerInterfaceCommon.LoggerModels
{

	[MetadataType(typeof(logchangeviewMetaData))]
	public partial class logchangeview
	{
		public EntityCommand EntityCommandEnum
		{
			get { return (EntityCommand)Action; }
			set { Action = (int)value; }
		}

		public string TypeDisplayName
		{
			get
			{
				return AttributeHelper.GetDisplayName(TypeName);
			}
		}
	}

	public class logchangeviewMetaData
	{
		[Display(Name = "Статус")]
		public EntityCommand EntityCommandEnum { get; }
	}

	public partial class propertychangeview
	{
		public string PropertyDisplayName
		{
			get
			{
				return AttributeHelper.GetDisplayName(TypeName, PropertyName);
			}
		}
	}


}

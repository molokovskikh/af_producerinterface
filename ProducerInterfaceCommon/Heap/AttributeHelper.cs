using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ProducerInterfaceCommon.Heap
{
	public static class AttributeHelper
	{
		public static string GetDisplayName(Type type)
		{
			var result = type.FullName;
			var displayName = (DisplayNameAttribute)Attribute.GetCustomAttribute(type, typeof(DisplayNameAttribute));
			if (displayName != null && !String.IsNullOrEmpty(displayName.DisplayName))
				result = displayName.DisplayName;

			return result;
		}

		public static string GetDisplayName(string typeName)
		{
			var result = typeName;
			var type = Type.GetType(typeName);
			if (type != null)
				result = GetDisplayName(type);

			return result;
		}

		public static string GetDisplayName(string typeName, string propertyName)
		{
			var result = propertyName;

			var type = Type.GetType(typeName);
			if (type == null)
				return result;

			var metadata = (MetadataTypeAttribute)Attribute.GetCustomAttribute(type, typeof(MetadataTypeAttribute));
			if (metadata == null)
				return result;

			var p = metadata.MetadataClassType.GetProperty(propertyName);
			if (p == null)
				return result;

			var da = p.GetCustomAttribute<DisplayAttribute>();
			if (da == null)
				return result;

			return da.Name;
		}
	}
}

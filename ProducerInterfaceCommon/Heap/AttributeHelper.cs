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

        public static string DisplayName(this Enum value)
        {
            Type enumType = value.GetType();
            var enumValue = Enum.GetName(enumType, value);
            MemberInfo member = enumType.GetMember(enumValue)[0];

            var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
            var outString = ((DisplayAttribute)attrs[0]).Name;

            if (((DisplayAttribute)attrs[0]).ResourceType != null)
            {
                outString = ((DisplayAttribute)attrs[0]).GetName();
            }

            return outString;
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

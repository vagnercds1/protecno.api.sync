using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace protecno.api.sync.domain.extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DisplayAttribute));

            if (displayAttribute != null)
            {
                return displayAttribute.Name;
            }

            return value.ToString();
        }

        public static string GetGroupName(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var displayAttribute = (DisplayAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DisplayAttribute));

            if (displayAttribute != null)
            {
                return displayAttribute.GroupName;
            }

            return string.Empty;
        }
    }


}
 
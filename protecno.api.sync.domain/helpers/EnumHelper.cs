using System;
using System.Globalization;

namespace protecno.api.sync.domain.helpers
{
    public static class EnumHelper
    {
        public static string GetEnumTitleCase<T>(T enumValue) where T : Enum
        {
            string enumName = Enum.GetName(typeof(T), enumValue);
            string titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(enumName);
            return titleCase;
        }
    }
}

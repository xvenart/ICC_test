using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TankCommon.Objects;

namespace TankCommon
{
    public static class Extensions
    {
        public static string GetDescription(this System.Enum enumObj)
        {
            if (enumObj == null)
            {
                throw new ArgumentNullException(nameof(enumObj));
            }

            var fieldName = enumObj.ToString();
            var fieldInfo = enumObj
                .GetType()
                .GetField(fieldName);

            return fieldInfo == null
                ? fieldName
                : fieldInfo
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .Select(x => x.Description)
                    .DefaultIfEmpty(fieldName)
                    .First();
        }

        public static string GetDescription(this TankSettings enumObj)
        {
            if (enumObj == null)
            {
                throw new ArgumentNullException(nameof(enumObj));
            }

            var fieldName = enumObj.ToString();
            var fieldInfo = enumObj
                .GetType()
                .GetField(fieldName);

            return fieldInfo == null
                ? fieldName
                : fieldInfo
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .Select(x => x.Description)
                    .DefaultIfEmpty(fieldName)
                    .First();
        }

        public static string GetDescription(this ServerSettings enumObj)
        {
            if (enumObj == null)
            {
                throw new ArgumentNullException(nameof(enumObj));
            }

            var fieldName = enumObj.ToString();
            var fieldInfo = enumObj
                .GetType()
                .GetField(fieldName);

            return fieldInfo == null
                ? fieldName
                : fieldInfo
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .Select(x => x.Description)
                    .DefaultIfEmpty(fieldName)
                    .First();
        }

        public static string GetDescription(this PropertyInfo enumObj)
        {
            if (enumObj == null)
            {
                throw new ArgumentNullException(nameof(enumObj));
            }

            return enumObj.GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .Cast<DescriptionAttribute>()
                            .Select(b => b.Description)
                            .DefaultIfEmpty(string.Empty)
                            .First();
        }
    }
}

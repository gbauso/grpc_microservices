using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoveryService.Extensions
{
    public static class UtilExtensions
    {
        public static T FromSection<T>(this IConfigurationSection section)
        {
            var instance = (T)Activator.CreateInstance(typeof(T));
            section.Bind(instance);

            return instance;
        }

        public static ICollection<string> SplitIfNotEmpty(this string str)
        {
            return string.IsNullOrEmpty(str) ? new List<string>() : str.Split(";").ToList<string>();
        }
    }
}

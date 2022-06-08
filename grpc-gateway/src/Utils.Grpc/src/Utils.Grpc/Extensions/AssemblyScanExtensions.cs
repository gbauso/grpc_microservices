using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.Grpc.Extensions
{
    internal static class AssemblyScanExtensions
    {
        public static IEnumerable<Type> GetGrpcClients(this AppDomain appDomain) =>
            appDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(i => i.Name.EndsWith("ServiceClient") && !i.Name.Contains("Discovery"));

        public static string GetServiceName(this Type client) =>
            client.ReflectedType.GetRuntimeFields().First().GetValue(null).ToString();

        
    }
}

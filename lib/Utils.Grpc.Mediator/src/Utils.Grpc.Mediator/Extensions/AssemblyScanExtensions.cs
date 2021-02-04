using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils.Grpc.Mediator.Extensions
{
    internal static class AssemblyScanExtensions
    {
        public static IEnumerable<Type> GetGrpcClients(this AppDomain appDomain) =>
            appDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(i => i.Name.EndsWith("ServiceClient") && !i.Name.Contains("Discovery"));

        public static string GetServiceName(this Type client) =>
            client.ReflectedType.GetRuntimeFields().First().GetValue(null).ToString();

        public static bool HasFullContent<Res>(this Res response) where Res : IMessage<Res> =>
            response.GetType().GetRuntimeFields()
                .Any(field => field.Name.EndsWith("_") 
                    && (field.GetValue(response) == null 
                        || string.IsNullOrEmpty(field.GetValue(response).ToString())));

        public static MethodType GetMethodType<Req, Res>(this Type client) =>
            ((Method<Req, Res>)client.ReflectedType
                            .GetRuntimeFields()
                            .Last().GetValue(null)).Type;

        public static IEnumerable<MethodInfo> GetCallableMethods(this Type client) =>
            client.GetMethods()
                    .Where(i => i.Name.EndsWith("Async") && i.GetParameters().Length == 2);

        public static MethodInfo GetMethodByResponse(this IEnumerable<MethodInfo> methods, Type response) =>
            methods.FirstOrDefault(i =>
                i.GetParameters().Length == 2 &&
                (i.ReturnType.FullName?.Contains(response.Name) ?? false));

        public static Type GetResponseType(this MethodInfo method) => method?.ReturnType.GenericTypeArguments[0];
    }
}

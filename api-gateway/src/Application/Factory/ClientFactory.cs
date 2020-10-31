using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Application.Exceptions;
using Application.GrpcClients.Interceptors;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Application.Factory
{
    public class ClientFactory
    {
        private readonly IDictionary<Type, (string service, Type client)> _clients;
        private readonly IDictionary<(Type, Channel), ClientBase> _instances;

        public ClientFactory()
        {
            _clients = new Dictionary<Type, (string service, Type client)>();
            _instances = new Dictionary<(Type, Channel), ClientBase>();
            Initialize();
        }

        public (string service, Type client) GetClientInfo(Type response)
        {
            if(!_clients.ContainsKey(response))
                throw new ClientNotFoundException();

            return _clients[response];
        }
        
        public ClientBase GetInstance(Type clientType, Channel channel)
        {
            var pair = (clientType, channel);

            if (_instances.ContainsKey(pair))
            {
                return _instances[pair];
            }
            else
            {
                var client = (ClientBase) Activator.CreateInstance(clientType, new object[] {channel });
                _instances[pair] = client;
                return client;
            }
        }

        private void Initialize()
        {
            var clients = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(i => i.Name.EndsWith("ServiceClient") && !i.Name.Contains("Discovery"));

            foreach (var client in clients)
            {
                var service = client.ReflectedType.GetRuntimeFields().First().GetValue(null).ToString();
                var returnType = client
                    .GetMethods()
                    .FirstOrDefault(i => i.Name.EndsWith("Async")
                                         && i.GetParameters().Length == 2)
                    ?.ReturnType.GenericTypeArguments[0];

                _clients.Add(returnType!, (service, client));
            }
        }
    }
}
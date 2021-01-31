using System;
using System.Collections.Generic;
using System.Linq;
using Utils.Grpc.Mediator.Exceptions;
using Utils.Grpc.Mediator.GrpcClients.Interceptors;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Utils.Grpc.Mediator.Extensions;
using System.Collections.Concurrent;

namespace Utils.Grpc.Mediator.Factory
{
    internal class ClientFactory
    {
        private readonly IDictionary<Type, ServiceClientPair> _clients;
        private readonly IDictionary<TypeChannelPair, ClientBase> _instances;

        private readonly MetricsInterceptor _MetricsInterceptor;

        public ClientFactory(MetricsInterceptor metricsInterceptor)
        {
            _clients = new ConcurrentDictionary<Type, ServiceClientPair>();
            _instances = new ConcurrentDictionary<TypeChannelPair, ClientBase>();
            _MetricsInterceptor = metricsInterceptor;
            Initialize();
        }

        public virtual ServiceClientPair GetClientInfo(Type response)
        {
            if (!_clients.ContainsKey(response) || !_clients.TryGetValue(response, out ServiceClientPair serviceClientPair))
                throw new ClientNotFoundException();

            return serviceClientPair;
        }

        public virtual ClientBase GetInstance(TypeChannelPair pair)
        {
            if (_instances.ContainsKey(pair) && _instances.TryGetValue(pair, out ClientBase clientBase))
            {
                return clientBase;
            }
            else
            {
                var client = (ClientBase)Activator.CreateInstance(pair.ServiceType,
                                                                   new object[] {
                                                                       pair.Channel.Intercept(_MetricsInterceptor)
                                                                   });
                _instances.TryAdd(pair, client);
                return client;
            }
        }

        private void Initialize()
        {
            var clients = AppDomain.CurrentDomain.GetGrpcClients();

            foreach (var client in clients)
            {
                var service = client.GetServiceName();
                var returnType = client.GetCallableMethods().FirstOrDefault().GetResponseType();

                var serviceClient = ServiceClientPair.Create(service, client);

                _clients.TryAdd(returnType, serviceClient);
            }
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;

namespace DiscoveryService
{
    public class EtcdClientWrap : EtcdClient
    {
        public EtcdClientWrap() : base("mock")
        {
            
        }

        public EtcdClientWrap(string connectionString,
                              int port = 2379,
                              string username = "",
                              string password = "",
                              string caCert = "",
                              string clientCert = "",
                              string clientKey = "",
                              bool publicRootCa = false) : base(connectionString, port, username, password, caCert, clientCert, clientKey, publicRootCa)
        {
        }

        public virtual string GetValue(string key)
        {
            var value = base.GetVal(key);

            return string.IsNullOrEmpty(value) ? null : value;
        }

        public virtual Task<PutResponse> PutValueAsync(KeyValuePair<string, string> kv)
        {
            return PutAsync(kv.Key, kv.Value);
        }
        
    }
}
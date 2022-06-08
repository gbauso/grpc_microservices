using Google.Protobuf;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utils.Grpc.Extensions
{
    public static class GrpcCallExtensions
    {
        public static CallOptions GetCallOptions<T>(this ClientBase<T> client, string correlationId, string method, string target) where T : ClientBase<T>
        {
            var headers = new Metadata
            {
                {"correlation_id", correlationId},
                {"service", typeof(T).GetServiceName() },
                {"rpc", method },
                {"target", target}
            };

            return new CallOptions(headers);
        }
        public static AsyncUnaryCall<T> CallWithRetry<T>(this AsyncUnaryCall<T> asyncUnaryCall)
        {
            return asyncUnaryCall;
        }

        public static async Task<T> MergeAllResults<T>(this IEnumerable<Task<T>> tasks) where T : IMessage<T>, new()
        {
            var results = await Task.WhenAll(tasks);
            var result = new T();

            foreach (var res in results)
            {
                result.MergeFrom(res);
            }

            return result;
        }
    }
}

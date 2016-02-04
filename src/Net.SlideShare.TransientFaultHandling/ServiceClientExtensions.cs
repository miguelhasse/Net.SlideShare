using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace System.Net.SlideShare
{
    public static class ServiceClientExtensions
    {
        private static readonly ITransientErrorDetectionStrategy DefaultTransientErrorDetectionStrategy;

        static ServiceClientExtensions()
        {
            DefaultTransientErrorDetectionStrategy = new TransientFaultHandling.Strategies.ServiceTransientErrorDetectionStrategy();
        }

        public static IReliableServiceClient AsReliable(this ServiceClient client, RetryStrategy retryStrategy)
        {
            return client.AsReliable(new RetryPolicy(DefaultTransientErrorDetectionStrategy, retryStrategy), new RetryPolicy(DefaultTransientErrorDetectionStrategy, retryStrategy));

        }

        public static IReliableServiceClient AsReliable(this ServiceClient client, RetryPolicy readRetryPolicy, RetryPolicy writeRetryPolicy)
        {
            return new TransientFaultHandling.Implementation.ReliableServiceClient(client, readRetryPolicy, writeRetryPolicy);
        }
    }
}

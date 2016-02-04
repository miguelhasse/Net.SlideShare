using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Diagnostics;

namespace System.Net.SlideShare.TransientFaultHandling.Strategies
{
    public class ServiceTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        bool ITransientErrorDetectionStrategy.IsTransient(Exception ex)
        {
            try
            {
                var serviceException = FindException<ServiceRequestException>(ex);

                if (serviceException != null)
                    return IsTransientServiceError(serviceException.Status);

                var webException = FindException<WebException>(ex);

                if (webException != null)
                {
#if !PORTABLE
                    if (webException.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = webException.Response as HttpWebResponse;
                        return (response != null && IsTransientHttpError(response.StatusCode));
                    }
#endif
                    return IsTransientWebError(webException.Status);
                }
            }
            catch (Exception ex2)
            {
#if PORTABLE
                Debug.WriteLine("Exception in transient error detection strategy: {0}", ex2);
#else
                Trace.TraceWarning("Exception in transient error detection strategy: {0}", ex2);
#endif
            }
            return false;
        }

        public virtual bool IsTransientServiceError(ServiceExceptionStatus statusCode)
        {
            return false;
        }

        public virtual bool IsTransientWebError(WebExceptionStatus statusCode)
        {
            switch (statusCode)
            {
                case WebExceptionStatus.SendFailure:
                case WebExceptionStatus.ConnectFailure:
#if !PORTABLE
                case WebExceptionStatus.ReceiveFailure:
                case WebExceptionStatus.PipelineFailure:
                case WebExceptionStatus.KeepAliveFailure:
#endif
                    return true;

                default:
                    return false;
            }
        }

        public virtual bool IsTransientHttpError(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.GatewayTimeout:
                    return true;

                default:
                    return false;
            }
        }

        private static T FindException<T>(Exception ex) where T : Exception
        {
            if (ex != null)
            {
                T exception = ex as T;
                return (exception != null) ? exception : FindException<T>(ex.InnerException);
            }
            return default(T);
        }
    }
}


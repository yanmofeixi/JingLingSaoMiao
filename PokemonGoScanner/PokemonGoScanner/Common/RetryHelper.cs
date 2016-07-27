namespace PokemonGoScanner.Common
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    internal class RetryHelper : DelegatingHandler
    {
        public RetryHelper(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            for (var i = 0; i <= Constant.NianticConnectionRetryCount; i++)
            {
                try
                {
                    var response = await base.SendAsync(request, cancellationToken);
                    if (response.StatusCode == HttpStatusCode.BadGateway)
                    {
                        Trace.TraceError("Bad Gateway, Niantic server down");
                        throw new Exception();
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error: {ex}");
                    if (i >= Constant.NianticConnectionRetryCount) throw;
                    await Task.Delay(Constant.NianticConnectionRetryDelayInMs, cancellationToken);
                }
            }
            return null;
        }
    }
}

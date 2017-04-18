using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Gateway.Handlers
{
    public class ApplicationInsightsTelemetryHandler : DelegatingHandler
    {
        private readonly TelemetryClient client;

        public ApplicationInsightsTelemetryHandler(TelemetryClient client)
        {
            this.client = client;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            var startDate = DateTimeOffset.Now;
            HttpStatusCode responseStatus = HttpStatusCode.InternalServerError;

            stopWatch.Start();

            try
            {                
                var response =  await base.SendAsync(request, cancellationToken);
                responseStatus = response.StatusCode;
                return response;
            }
            catch (Exception e)
            {
                TrackException(e);
                throw;
            }
            finally
            {
                stopWatch.Stop();
                TrackRequest(request, responseStatus, startDate, stopWatch.Elapsed);
            }
        }

        private void TrackException(Exception ex)
        {
            client.TrackException(ex);
        }

        private void TrackRequest(HttpRequestMessage request, HttpStatusCode responseStatus, DateTimeOffset startDate, TimeSpan duration)
        {
            var method = request.Method.ToString().ToUpper();
            var name = $"{method} {request.RequestUri}";
            var statusCode = (int) responseStatus;

            var requestTelemetry = new RequestTelemetry(name, startDate, duration, statusCode.ToString(), statusCode < 400);
            client.TrackRequest(requestTelemetry);
        }
    }
}

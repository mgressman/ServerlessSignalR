using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace SignalRService.Infrastructure.Extensions
{
    public static class FunctionContextExtensions
    {
        public static async Task SetHttpResponseStatusCode(this FunctionContext context, HttpStatusCode statusCode,
            string responseValue = "")
        {
            var httpRequestData = await context.GetHttpRequestDataAsync();

            if (httpRequestData != null)
            {
                // create an instance of httpresponsedata with status code
                var newHttpResponse = httpRequestData.CreateResponse(statusCode);
                await newHttpResponse.WriteStringAsync(responseValue);

                // update invocation result
                context.GetInvocationResult().Value = newHttpResponse;
            }
        }
    }
}

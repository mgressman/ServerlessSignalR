using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MediatR;
using SignalRService.ApplicationCore.Commands.Negotiate;
using SignalRService.ApplicationCore.Commands.Connection;

namespace SignalRService.Api.Controllers
{
    public class SignalRController(
        ILogger<SignalRController> logger,
        IMediator mediator)
    {
        #region Fields

        private const string HubName = "DefaultHub"; // Used by SignalR trigger only

        #endregion
        
        #region Methods

        // as long as the authenticationmiddleware is registered in the startup,
        // the negotiate endpoint will be protected by the middleware and a token
        // will be required to connect to the hub.
        [Function(nameof(Negotiate))]
        public async Task<HttpResponseData> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var command = new NegotiateConnection();

            if (command == null)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }

            var negotiateResponse = await mediator.Send(command);

            var response = req.CreateResponse();
            await response.WriteBytesAsync(negotiateResponse.ToArray());
            return response;
        }

        [Function(nameof(OnConnected))]
        public async Task OnConnected(
            [SignalRTrigger(HubName, "connections", "connected")]
            SignalRInvocationContext invocationContext)
        {
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            
            var command = new ConnectClient(invocationContext);

            await mediator.Send(command);
        }

        [Function(nameof(OnDisconnected))]
        public async Task OnDisconnected(
            [SignalRTrigger(HubName, "connections", "disconnected")]
            SignalRInvocationContext invocationContext)
        {
            logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");

            var command = new DisconnectClient(invocationContext);

            await mediator.Send(command);
        }

        #endregion

    }
}

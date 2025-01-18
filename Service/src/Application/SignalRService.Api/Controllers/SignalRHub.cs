using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.SignalRService;
using Microsoft.Extensions.Logging;
using SignalRService.ApplicationCore;

namespace SignalRService.Api.Controllers
{
    public class SignalRHub(
        ILogger<SignalRHub> logger,
        IServiceProvider serviceProvider)
        : ServerlessHub<ISignalRClient>(serviceProvider)
    {
        #region Fields

        private const string HubName = nameof(SignalRHub); // Used by SignalR trigger only

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

            var negotiateResponse = await NegotiateAsync();
            var response = req.CreateResponse();
            await response.WriteBytesAsync(negotiateResponse.ToArray());
            return response;
        }

        [Function(nameof(OnConnected))]
        public Task OnConnected(
            [SignalRTrigger(HubName, "connections", "connected")]
            SignalRInvocationContext invocationContext)
        {
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            return Clients.All.NewConnection(new NewConnection(invocationContext.ConnectionId));
        }

        [Function("Broadcast")]
        [SignalROutput(HubName = "Hub")]
        public SignalRMessageAction Broadcast([SignalRTrigger(HubName, "messages", "Broadcast", "message")] SignalRInvocationContext invocationContext, string message)
        {
            return new SignalRMessageAction("newMessage")
            {
                Arguments = new object[] { new NewMessage(invocationContext, message) }
            };
        }

        [Function(nameof(OnDisconnected))]
        public SignalRMessageAction OnDisconnected([SignalRTrigger(HubName, "connections", "disconnected")] SignalRInvocationContext invocationContext)
        {
            invocationContext.Headers.TryGetValue("Authorization", out var auth);
            logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
            return new SignalRMessageAction("oldConnection")
            {
                Arguments = [new NewConnection(invocationContext.ConnectionId)]
            };
        }

        #endregion

        ////private async Task<bool> ValidateTokenAsync(HttpRequestData req)
        ////{
        ////    if (!TryGetTokenFromHeaders(req, logger, out var token))
        ////    {
        ////        return false;
        ////    }

        ////    if (!_tokenValidator.CanReadToken(token))
        ////    {
        ////        // token is malformed
        ////        await context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
        ////        return;
        ////    }

        ////    // get openid connect metadata
        ////    var validationParameters = _tokenValidationParameters.Clone();

        ////    var openIdConfig = await _configurationManager.GetConfigurationAsync(default);

        ////    validationParameters.ValidIssuer = openIdConfig.Issuer;
        ////    validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

        ////    try
        ////    {
        ////        // validate token
        ////        var principal = _tokenValidator.ValidateToken(
        ////            token, validationParameters, out _);

        ////        // set principal + token in features collection.
        ////        // they can be accessed from here later in the call chain.
        ////        context.Features.Set(new JwtPrincipalFeature(principal, token));

        ////        await next(context);
        ////    }
        ////    catch (SecurityTokenException)
        ////    {
        ////        // token is not valid (expired etc.)
        ////        await context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
        ////    }
        ////}

        ////private static bool TryGetTokenFromHeaders(HttpRequestData req, ILogger logger, out string? token)
        ////{
        ////    token = null;

        ////    if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
        ////    {
        ////        logger.LogWarning("Authorization header not found.");
        ////        return false;
        ////    }

        ////    token = authHeaders.FirstOrDefault()?.Split(" ").Last();

        ////    if (!string.IsNullOrEmpty(token))
        ////    {
        ////        return true;
        ////    }

        ////    logger.LogWarning("Bearer token not found.");
        ////    return false;
        ////}

        public class NewMessage
        {
            public string ConnectionId { get; }
            public string Sender { get; }
            public string Text { get; }

            public NewMessage(SignalRInvocationContext invocationContext, string message)
            {
                Sender = string.IsNullOrEmpty(invocationContext.UserId) ? string.Empty : invocationContext.UserId;
                ConnectionId = invocationContext.ConnectionId;
                Text = message;
            }
        }
    }
}

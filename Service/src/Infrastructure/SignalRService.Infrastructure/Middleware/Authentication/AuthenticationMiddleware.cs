using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SignalRService.Infrastructure.Extensions;

namespace SignalRService.Infrastructure.Middleware.Authentication
{
    public class AuthenticationMiddleware : IFunctionsWorkerMiddleware
    {
        #region Fields

        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly JwtSecurityTokenHandler _tokenValidator;
        private readonly TokenValidationParameters _tokenValidationParameters;

        #endregion

        #region Constructors

        public AuthenticationMiddleware(IConfiguration configuration)
        {
            var authority = configuration["AuthenticationAuthority"];
            var audience = configuration["AuthenticationClientId"];

            _tokenValidator = new JwtSecurityTokenHandler();

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidAudience = audience
            };
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
        }

        #endregion

        #region Methods

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (!TryGetTokenFromHeaders(context, out var token))
            {
                // unable to get token from headers
                _ = context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // token is malformed
                await context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
                return;
            }

            // get openid connect metadata
            var validationParameters = _tokenValidationParameters.Clone();

            var openIdConfig = await _configurationManager.GetConfigurationAsync(default);

            validationParameters.ValidIssuer = openIdConfig.Issuer;
            validationParameters.IssuerSigningKeys = openIdConfig.SigningKeys;

            try
            {
                // validate token
                var principal = _tokenValidator.ValidateToken(
                    token, validationParameters, out _);

                // set principal + token in features collection.
                // they can be accessed from here later in the call chain.
                context.Features.Set(new JwtPrincipalFeature(principal, token));

                await next(context);
            }
            catch (SecurityTokenException)
            {
                // token is not valid (expired etc.)
                await context.SetHttpResponseStatusCode(HttpStatusCode.Unauthorized);
            }
        }

        private static bool TryGetTokenFromHeaders(FunctionContext context, out string? token)
        {
            token = null;

            // http headers are in the binding context as a json object.
            // the first checks ensure that we have the json string.
            if (!context.BindingContext.BindingData.TryGetValue("Headers", out var headersObject))
            {
                return false;
            }

            if (headersObject is not string headersString)
            {
                return false;
            }

            // deserialize headers from json
            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersString);

            if (headers == null)
            {
                return false;
            }

            var normalizedKeyHeaders =
                headers.ToDictionary(header => header.Key.ToLowerInvariant(), header => header.Value);

            if (!normalizedKeyHeaders.TryGetValue("authorization", out var authorizationHeaderValue))
            {
                // No Authorization header present
                return false;
            }

            if (!authorizationHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                // scheme is not bearer
                return false;
            }

            token = authorizationHeaderValue["Bearer ".Length..].Trim();
            return true;
        }

        #endregion
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationFramework.Web.Core.AzureFunction.Authentication.Token
{
    public class JwtToken : IJwtToken
    {
        #region Fields

        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
        private readonly JwtSecurityTokenHandler _tokenValidator;
        private readonly TokenValidationParameters _tokenValidationParameters;

        #endregion

        #region Constructors

        public JwtToken(IConfiguration configuration)
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

        public async Task<ClaimsPrincipal?> ValidateFromRequest(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request)
        {
            if (!TryGetTokenFromHeaders(request, out var token))
            {
                // unable to get token from headers
                return null;
            }

            if (!_tokenValidator.CanReadToken(token))
            {
                // token is malformed
                return null;
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

                return principal;
            }
            catch (SecurityTokenException)
            {
                // token is not valid (expired etc.)
                return null;
            }
        }

        private static bool TryGetTokenFromHeaders(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request, out string? token)
        {
            token = null;

            if (!request.Headers.TryGetValues("Authorization", out var authHeaders))
            {
                // no authorization header present
                return false;
            }

            var authHeader = authHeaders.FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                // schema is not bearer
                return false;
            }

            // extract the token
            token = authHeader["Bearer ".Length..].Trim();

            return true;
        }

        #endregion
    }
}
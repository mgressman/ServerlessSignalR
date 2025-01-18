using System.Security.Claims;

namespace SignalRService.Infrastructure.Middleware
{
    public class JwtPrincipalFeature
    {
        #region Properties

        public string AccessToken { get; }

        public ClaimsPrincipal Principal { get; }

        #endregion

        #region Constructors

        public JwtPrincipalFeature(ClaimsPrincipal principal, string accessToken)
        {
            Principal = principal;
            AccessToken = accessToken;
        }

        #endregion
    }
}

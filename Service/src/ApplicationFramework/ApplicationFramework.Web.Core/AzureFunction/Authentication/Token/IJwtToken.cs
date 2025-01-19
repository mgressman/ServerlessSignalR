using System.Security.Claims;

namespace ApplicationFramework.Web.Core.AzureFunction.Authentication.Token
{
    public interface IJwtToken
    {
        #region Methods

        Task<ClaimsPrincipal?> ValidateFromRequest(Microsoft.Azure.Functions.Worker.Http.HttpRequestData request);

        #endregion
    }
}

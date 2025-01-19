using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace SignalRService.ApplicationCore.Commands.Connection
{
    public class DisconnectClient : IRequest<Unit>
    {
        #region Properties

        public SignalRInvocationContext InvocationContext { get; set; }

        #endregion

        #region Constructors

        public DisconnectClient(SignalRInvocationContext invocationContext)
        {
            InvocationContext = invocationContext;
        }

        #endregion
    }
}

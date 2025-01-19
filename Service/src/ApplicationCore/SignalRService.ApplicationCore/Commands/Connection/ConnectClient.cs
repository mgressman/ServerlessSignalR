using MediatR;
using Microsoft.Azure.Functions.Worker;

namespace SignalRService.ApplicationCore.Commands.Connection
{
    public class ConnectClient : IRequest<Unit>
    {
        #region Properties

        public SignalRInvocationContext InvocationContext { get; set; }

        #endregion

        #region Constructors

        public ConnectClient(SignalRInvocationContext invocationContext)
        {
            InvocationContext = invocationContext;
        }

        #endregion
    }
}

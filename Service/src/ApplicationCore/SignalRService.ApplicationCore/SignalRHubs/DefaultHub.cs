using MediatR;
using Microsoft.Azure.Functions.Worker.SignalRService;
using SignalRService.ApplicationCore.Commands.Connection;
using SignalRService.ApplicationCore.Commands.Negotiate;
using SignalRService.ApplicationCore.Interfaces.SignalR;

namespace SignalRService.ApplicationCore.SignalRHubs
{
    public class DefaultHub(
        IServiceProvider serviceProvider) : 
        ServerlessHub<IDefaultHub>(serviceProvider),
        IRequestHandler<NegotiateConnection, BinaryData>,
        IRequestHandler<ConnectClient, Unit>,
        IRequestHandler<DisconnectClient, Unit>
    {
        #region Methods
        
        public async Task<BinaryData> Handle(NegotiateConnection request, CancellationToken cancellationToken)
        {
            var negotiateResponse = await NegotiateAsync();

            return negotiateResponse;
        }

        #endregion

        public async Task<Unit> Handle(ConnectClient command, CancellationToken cancellationToken)
        {
            await Clients.All.ClientConnected(new ConnectionInformation(command.InvocationContext.ConnectionId));

            return Unit.Value;
        }

        public async Task<Unit> Handle(DisconnectClient command, CancellationToken cancellationToken)
        {
            await Clients.All.ClientDisconnected(new ConnectionInformation(command.InvocationContext.ConnectionId));

            return Unit.Value;
        }
    }
}

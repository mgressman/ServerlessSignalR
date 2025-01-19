using MediatR;

namespace SignalRService.ApplicationCore.Interfaces.SignalR
{
    public interface IDefaultHub
    {
        #region Methods

        Task ClientConnected(ConnectionInformation connection);

        Task ClientDisconnected(ConnectionInformation connection);

        #endregion
    }
}

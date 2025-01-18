namespace SignalRService.ApplicationCore
{
    #region Methods

    public interface ISignalRClient
    {
        Task NewConnection(NewConnection connection);
    }

    #endregion
}

namespace SignalRService.ApplicationCore
{
    public class ConnectionInformation(string connectionId)
    {
        public string ConnectionId { get; } = connectionId;
    }
}

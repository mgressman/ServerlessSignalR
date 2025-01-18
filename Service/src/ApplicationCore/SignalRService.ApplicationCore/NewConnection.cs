namespace SignalRService.ApplicationCore
{
    public class NewConnection(string connectionId)
    {
        public string ConnectionId { get; } = connectionId;
    }
}

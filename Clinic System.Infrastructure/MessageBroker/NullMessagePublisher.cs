namespace Clinic_System.Infrastructure.MessageBroker
{
    /// <summary>No-op publisher for local development without RabbitMQ.</summary>
    public class NullMessagePublisher : IMessagePublisher
    {
        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            => Task.CompletedTask;
    }
}

namespace Munan.RabbitMqEvent.Handlers;

public interface IMqHandler<TMessage>
{
    Task Handle(TMessage message, CancellationToken cancellationToken);
}

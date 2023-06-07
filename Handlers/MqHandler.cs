namespace Munan.RabbitMqEvent.Handlers;

public abstract class MqHandler<TMessage> : IMqHandler<TMessage> where TMessage : IMqEventParam
{
    Task IMqHandler<TMessage>.Handle(TMessage notification, CancellationToken cancellationToken)
    {
        Handle(notification);
        return Task.CompletedTask;
    }
    public abstract void Handle(TMessage message);
}

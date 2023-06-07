namespace Munan.RabbitMqEvent.Models;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string QueueName { get; set; }

    public string ExchangeName { get; set; }

    public string RoutingKey { get; set; }

    public RabbitMqOptions(string? _queueName)
    {
        QueueName = _queueName ?? "_MyRabbitMqQueue";
        ExchangeName = $"{QueueName}-exchange";
        RoutingKey = $"{QueueName}-key";
    }
}

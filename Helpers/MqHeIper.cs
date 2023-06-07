using Munan.RabbitMqEvent.Models;
using RabbitMQ.Client;
using System.Text.Json;

namespace Munan.RabbitMqEvent.Helpers;

public class MqHeIper
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly RabbitMqOptions _options;

    public MqHeIper(ConnectionFactory connectionFactory, RabbitMqOptions options)
    {
        _connectionFactory = connectionFactory;
        _options = options;
    }

    public void Publish<T>(T data)
    {
        if (_connectionFactory != null && data != null)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var _channel = conn.CreateModel();
            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(new MqBody(typeof(T).FullName!, data, typeof(T).FullName!));

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: _options.RoutingKey,
                mandatory: true, basicProperties: properties, body: body); // 发布消息  
        }
    }
}

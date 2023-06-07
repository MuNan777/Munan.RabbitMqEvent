using Microsoft.Extensions.Hosting;
using Munan.RabbitMqEvent.Models;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Munan.RabbitMqEvent;

internal class MqConsumeHostedService : BackgroundService
{
  private readonly ConnectionFactory _connectionFactory;

  private readonly RabbitMqOptions _options;

  private IModel? _channel = null;

  private IConnection? _connection = null;

  private readonly Dictionary<string, Type> _paramTypeMap;


  // 用于保存 MethodInfo
  private readonly Dictionary<string, List<ConsumeHandleItem>> _consumeHandle;

  private readonly IServiceProvider _serviceProvider;

  public MqConsumeHostedService(
      ConnectionFactory connectionFactory,
      RabbitMqOptions options,
      Dictionary<string, List<ConsumeHandleItem>> consumeHandle,
      IServiceProvider serviceProvider,
      Dictionary<string, Type> paramTypeMap)
  {
    _connectionFactory = connectionFactory;
    _options = options;
    _serviceProvider = serviceProvider;
    _consumeHandle = consumeHandle;
    InitRabbitMq();
    _paramTypeMap = paramTypeMap;
  }


  private void InitRabbitMq()
  {
    _connection = _connectionFactory.CreateConnection();
    _channel = _connection.CreateModel();
    // 设置消息的持久性
    var properties = _channel.CreateBasicProperties();
    properties.DeliveryMode = 2; // 持久化到磁盘上

    var exchange = _options.ExchangeName;
    var queue = _options.QueueName;
    var routingKey = _options.RoutingKey;
    _channel.ExchangeDeclare(exchange, type: "direct"); // 声明交换机
    _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
    _channel.QueueBind(queue, exchange, routingKey);
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    if (_channel != null)
    {
      var consumer = new EventingBasicConsumer(_channel);

      consumer.Received += (model, ea) =>
      {
        try
        {
          var body = ea.Body.ToArray();
          var result = JsonSerializer.Deserialize<MqBody>(body);
          if (result != null && _consumeHandle.TryGetValue(result.Key, out var list))
          {
            var paramType = _paramTypeMap[result.ParamTypeFullName];
            var dataOrg = result.Data.ToString();
            if (paramType != null && dataOrg != null)
            {
              var data = JsonSerializer.Deserialize(dataOrg, paramType);
              list.ForEach(item =>
                    {
                        object handleInstance = _serviceProvider.GetRequiredService(item.Type);
                        if (data != null)
                        {
                          Task.Run(() =>
                                {
                                item.Action.Invoke(handleInstance, new object[] { data });
                              });
                        }
                      });
            }
          };

          // 手动确认消息已经被消费
          _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
        }
      };
      _channel.BasicConsume(queue: _options.QueueName, autoAck: false, consumer);
    }
    return Task.CompletedTask;
  }

  public override void Dispose()
  {
    _connection?.Dispose();
    _channel?.Dispose();
    base.Dispose();
  }
}

using Munan.RabbitMqEvent;
using Munan.RabbitMqEvent.Handlers;
using Munan.RabbitMqEvent.Helpers;
using Munan.RabbitMqEvent.Models;
using RabbitMQ.Client;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRabbitMqEvent(this IServiceCollection services)
  {
    var factory = new ConnectionFactory();
    var assembly = Assembly.GetCallingAssembly();
    var queue = assembly.GetName().Name;
    var options = new RabbitMqOptions(queue);
    factory.HostName = options.HostName;
    factory.UserName = options.UserName;
    factory.Password = options.Password;
    services.AddSingleton(factory);
    services.AddSingleton(options);
    services.AddScoped<MqHeIper>();
    InitConsume(services, assembly);
    HandleParam(services, assembly);
    services.AddHostedService<MqConsumeHostedService>();
    return services;
  }

  public static IServiceCollection AddRabbitMqEvent(this IServiceCollection services, Func<RabbitMqOptions, RabbitMqOptions> callBack)
  {
    var factory = new ConnectionFactory();
    var assembly = Assembly.GetCallingAssembly();
    var queue = assembly.GetName().Name;
    var options = callBack.Invoke(new RabbitMqOptions(queue));
    factory.HostName = options.HostName;
    factory.UserName = options.UserName;
    factory.Password = options.Password;
    services.AddSingleton(factory);
    services.AddSingleton(options);
    services.AddScoped<MqHeIper>();
    InitConsume(services, assembly);
    HandleParam(services, assembly);
    services.AddHostedService<MqConsumeHostedService>();
    return services;
  }

  private static void HandleParam(IServiceCollection services, Assembly assembly)
  {
    var ParamTypeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    var types = assembly.GetTypes().Where(
        t =>
        t.IsClass && // 类型实例
        !t.IsAbstract && // 排除抽象类，只保留可实例化的类类型
        t.GetInterfaces().Contains(typeof(IMqEventParam)) // 实现了 IMqEventParam 接口
        ).ToArray();
    foreach (var type in types)
    {
      if (type.FullName != null)
      {
        ParamTypeMap.Add(type.FullName, type);
      }
    }
    services.AddSingleton(ParamTypeMap);
  }

  private static void InitConsume(IServiceCollection services, Assembly assembly)
  {
    var ConsumeHandle = new Dictionary<string, List<ConsumeHandleItem>>(StringComparer.OrdinalIgnoreCase);

    var types = assembly.GetTypes().Where(
        t =>
        t.IsClass && // 类型实例
        !t.IsAbstract && // 排除抽象类，只保留可实例化的类类型
        t.BaseType != null && // 有父类
        t.BaseType.IsGenericType && // 父类是泛型类型
        t.BaseType.GetGenericTypeDefinition() == typeof(MqHandler<>) && // 父类是MyMqHandler<T>类型
        t.BaseType.GetGenericArguments()[0].GetInterfaces().Contains(typeof(IMqEventParam)) // 父类的泛型参数实现了IMyMq接口
        ).Select(t => new
        {
          Type = t,
          GenericType = t.BaseType!.GetGenericArguments()[0]
        }).ToArray();
    foreach (var type in types)
    {
      var handleMethod = type.Type.GetMethod("Handle");
      if (handleMethod != null && type.GenericType.FullName != null)
      {
        var key = type.GenericType.FullName;
        if (ConsumeHandle.TryGetValue(key, out var list))
        {
          list.Add(new ConsumeHandleItem(type.Type, handleMethod));
        }
        else
        {
          ConsumeHandle.Add(key, new List<ConsumeHandleItem> { new ConsumeHandleItem(type.Type, handleMethod) });
        }
        services.AddSingleton(type.Type);
      }
    }
    services.AddSingleton(ConsumeHandle);
  }
}
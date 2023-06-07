# Munan.RabbitMqEvent
A simple RabbitMq event handling plugin / 一个简单的 RabbitMq 事件处理插件

Just one function / 就一个功能

Realize that producers and consumers match by parameter type / 实现生产者和消费者通过参数类型进行匹配

### Example

setting / 配置

```json
{
    "RabbitMq": {
        "Host": "localhost",
        "UserName": "guest",
        "Password": "guest"
     }
}
```

```c#
builder.Services.AddRabbitMqEvent(opt => {
    var config = builder.Configuration.GetSection("RabbitMq");
    opt.HostName = config["Host"];
    opt.UserName = config["UserName"];
    opt.Password = config["Password"];
    return opt;
});
```

Or define your own / 或者你自己定义

```c#
// This is the configuration class / 这个是配置类
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
```

Parameter class / 参数类

```c#
public record TestMqObj(int Id, string Name) : IMqEventParam;
```

Consumer / 消费者

```c#
// It is necessary to inherit MqHandler and implement the Handle method
// 需要继承 MqHandler，实现 Handle 方法
// Support dependency injection, type is singleton
// 支持依赖注入，类型是单例
public class TestMqHandler : MqHandler<TestMqObj>
{

    private readonly IServiceProvider _services;
	 
    public TestMqHandler(IServiceProvider services)
    {
        _services = services;
    }

    public override void Handle(TestMqObj message)
    {
        Thread.Sleep(1000);
        using var scope = _services.CreateScope();
        var di2 = scope.ServiceProvider.GetRequiredService<TestDI2>();
        /*
        builder.Services.AddScoped<TestDI2>();
        public class TestDI2
        {
            public string Text { get; set; }

            public TestDI2()
            {
                Text = "TestDI2";
            }
        }
        */
        Console.WriteLine($"{message.Id} : {message.Name} {di2.Text}");
    }
}

public class TestMqHandler2 : MqHandler<TestMqObj>
{
    private readonly TestDI _TestDI;

    public TestMqHandler2(TestDI testDI)
    {
        _TestDI = testDI;
    }
    /*
    builder.Services.AddSingleton<TestDI>();
    public class TestDI
    {
        public string Text { get; set; }

        public TestDI()
        {
            Text = "TestDI";
        }
    }
    */

    public override void Handle(TestMqObj message)
    {
        Console.WriteLine($"{message.Id} : {message.Name} 2 {_TestDI.Text}");
    }
}
```

Producer / 生产者

```c#
private readonly MqHeIper _mqHeIper;

public TestController(MqHeIper mqHeIper)
{
    _rabbitMqHeIper = rabbitMqHeIper;
}

[HttpGet]
public async Task<ActionResult> TestEvent () {
    // Note the parameter type here TestMqObj
    // 注意这里的参数类型 TestMqObj
    _mqHeIper.Publish(new TestMqObj(1, "abc"));
    return Ok("ok");
}
```

That'll be fine  / 这样就行了
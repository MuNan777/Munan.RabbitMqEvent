namespace Munan.RabbitMqEvent.Models;

internal class MqBody
{
    public string Key { get; set; }

    public object Data { get; set; }

    public string ParamTypeFullName { get; set; }

    public MqBody(string key, object data, string paramTypeFullName)
    {
        Key = key;
        Data = data;
        ParamTypeFullName = paramTypeFullName;
    }
}

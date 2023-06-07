using System.Reflection;

namespace Munan.RabbitMqEvent.Models;

internal record ConsumeHandleItem(Type Type, MethodInfo Action);

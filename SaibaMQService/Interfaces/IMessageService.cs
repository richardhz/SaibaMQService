using System;

namespace SaibaMQService.Interfaces
{
    public interface IMessageService
    {
        void SendTopic<T>(string routingKey, T data) where T : class;
        void InitTopicConsumer<T>(string routingKey, Action<T> process) where T : class;
    }
}

using System;

namespace SaibaMQService.Interfaces
{
    public interface IMessageService
    {
        void TopicSend<T>(T data, string routingKey, string reference = "none") where T : class;
        void InitTopicConsumer<T>(string routingKey, Action<T> process) where T : class;
        void TopicListen();

    }
}

using System;

namespace SaibaMQService.Interfaces
{
    public interface IMessageService
    {
        void TopicAddQueue(string queueName, string routingKey);
        void TopicSend<T>( T data, string queueName, string routingKey, string reference = "none") where T : class;
        void InitTopicConsumer<T>(string queueName, string routingKey, Action<T> process) where T : class;
        void TopicListen();

    }
}

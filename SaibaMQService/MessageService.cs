using SaibaMQService.Interfaces;
using System;

namespace SaibaMQService
{
    public class MessageService : IMessageService
    {
        public void InitTopicConsumer<T>(string routingKey, Action<T> process) where T : class
        {
            throw new NotImplementedException();
        }

        public void SendTopic<T>(string routingKey, T data) where T : class
        {
            throw new NotImplementedException();
        }
    }
}

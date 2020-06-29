using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SaibaMQService.Interfaces;
using SaibaMQService.Models;
using System;
using System.Text;

namespace SaibaMQService
{
    public class MessageService : IMessageService
    {
        private readonly string exchangeName;
        private readonly string hostName;
        private readonly bool isDurable;
        private readonly bool canAutoDelete;

        private IModel channel;
        private EventingBasicConsumer consumer;
        private string consumerQueueName;
        private bool isListening;


        public MessageService(string exchangeName = "SbrDefault", string hostName = "", bool isDurable = false, bool isExclusive = false, bool canAutoDelete = false)
        {
            this.exchangeName = exchangeName;
            this.hostName = hostName;
            this.isDurable = isDurable;
            this.canAutoDelete = canAutoDelete;
        }

        public void InitTopicConsumer<T>(string routingKey, Action<T> process) where T : class
        {
            bool returnMessageTransport = routingKey.Contains("*");

            var connection = GetConnection(hostName);
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName,
                                    type: "topic",
                                    durable: isDurable,
                                    autoDelete: canAutoDelete,
                                    arguments: null);  //We need to allow arguments

            consumerQueueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: consumerQueueName,
                                exchange: exchangeName,
                                routingKey: routingKey);

            consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var data = Encoding.UTF8.GetString(body.ToArray());
                if (returnMessageTransport)
                {
                    var messageTransport = JsonConvert.DeserializeObject<T>(data);
                    process.Invoke(messageTransport);
                }
                else
                {
                    var messageTransport = JsonConvert.DeserializeObject<MessageTransport>(data);
                    var payload = Encoding.UTF8.GetString(messageTransport.Payload);
                    var obj = JsonConvert.DeserializeObject<T>(payload);
                    process.Invoke(obj);
                }
            };
        }

        public void TopicListen() //We need to allow manual acknowledge.
        {
            if (channel != null && !isListening)
            {
                channel.BasicConsume(queue: consumerQueueName,
                                autoAck: true,
                                consumer: consumer);
                isListening = true;
            }
        }

        public void TopicSend<T>(T data, string routingKey, string reference = "none" ) where T : class  //We need to allow arguments
        {
            using var connection = GetConnection(hostName);
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(exchange: exchangeName,
                                    type: "topic",
                                    durable: isDurable,
                                    autoDelete: canAutoDelete,
                                    arguments: null);



            var message = GenerateMessageTransport(data, reference);

            var content = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(content);

            channel.BasicPublish(exchange: exchangeName,
                                        routingKey: routingKey,
                                        basicProperties: null,
                                        body: body);
        }


        private MessageTransport GenerateMessageTransport<T>(T message, string reference = "none") where T : class
        {
            var messageTransport = new MessageTransport
            {
                Reference = reference,
                MessageDate = DateTimeOffset.Now,
                MessageType = message.GetType().Name
            };
            var output = JsonConvert.SerializeObject(message);
            messageTransport.Payload = Encoding.UTF8.GetBytes(output);

            return messageTransport;
        }

        private IConnection GetConnection(string host = "")
        {
            var factory = new ConnectionFactory
            {
                //factory.UserName = "rhz"; 
                //factory.Password = "dragon57";
                //factory.VirtualHost = "/";
                
                HostName = host,
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            return factory.CreateConnection();
        }
    }
}

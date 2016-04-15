using MassTransit;
using MassTransitChat.Messages;
using System;
using System.Threading.Tasks;

namespace MassTransitChat
{
    public class ChatConsumer :
        IConsumer<ChatMessage>,
        IConsumer<WhosThereRequest>,
        IConsumer<PlayerLeftChat>
    {
        private readonly string _name;
        public ChatConsumer(string name)
        {
            _name = name;
        }

        /// <summary>
        /// This method gets called (by MassTransit) when a new 'ChatMessage' arrves
        /// </summary>
        /// <param name="messageContext">MassTransit gives this to you, for supporting retry, etc</param>
        public Task Consume(ConsumeContext<ChatMessage> messageContext)
        {
            return Task.Run(() =>
            {
                var message = messageContext.Message;
                Console.WriteLine("{0} [{1}]: {2}", DateTime.Now, message.From, message.Message);
            });
        }

        /// <summary>
        /// This method gets called (by MassTransit) when a new 'WhosThereRequest' arrives
        /// </summary>
        /// <param name="messageContext">MassTransit gives this to you, for supporting retry, etc</param>
        public Task Consume(ConsumeContext<WhosThereRequest> messageContext)
        {
            return Task.Run(() =>
            {
                var message = messageContext.Message;

                if (!_name.Equals(message.Name)) //A copy of this will also be delivered to the sender, so lets filter that out
                {
                    Console.WriteLine("{0} is asking who's there", message.Name);
                }

                var response = new WhosThereResponse
                {
                    Name = _name
                };

                messageContext.Respond(response);
            });
        }

        public Task Consume(ConsumeContext<PlayerLeftChat> context)
        {
            return Task.Run(() =>
            {
                Console.WriteLine("{0} has left the chat.", context.Message.Name);
            });
        }
    }
}
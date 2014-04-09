using MassTransit;
using MassTransitChat.Messages;
using System;

namespace MassTransitChat
{
    public class ChatConsumer : Consumes<ChatMessage>.Context, Consumes<WhosThereRequest>.Context
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
        public void Consume(IConsumeContext<ChatMessage> messageContext)
        {
            var message = messageContext.Message;
            Console.WriteLine("{0} [{1}]: {2}", DateTime.Now, message.From, message.Message);
        }


        /// <summary>
        /// This method gets called (by MassTransit) when a new 'WhosThereRequest' arrives
        /// </summary>
        /// <param name="messageContext">MassTransit gives this to you, for supporting retry, etc</param>
        public void Consume(IConsumeContext<WhosThereRequest> messageContext)
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
        }
    }
}

using MassTransit;
using MassTransit.Log4NetIntegration;
using MassTransitChat.Messages;
using System;

namespace MassTransitChat
{
    class Program
    {
        private const string QueueNameFormat = "rabbitmq://localhost/chat_{0}";
        static void Main(string[] args)
        {
            Console.WriteLine("What is your name?");
            var name = Console.ReadLine();

            Console.Clear();

            Console.WriteLine("Connecting to chat...");


            /*
             *  This creates a new IServiceBus instance
             *  The IServiceBus gets your code connected to the routing fabric
             *  and allows you to publish and consume messages
             */
            var serviceBus = ServiceBusFactory.New(sbc =>
            {
                sbc.UseRabbitMq(); //Sets the transport "driver" to RabbitMQ

                //This is how you specify your address
                //This is also how you configure message consumption strategies
                //While the rules are implcit, they are powerful
                //Two services running at the same 'name' compete for messages
                //Services running on different names each get a copy of the message
                sbc.ReceiveFrom(string.Format(QueueNameFormat, name));

                sbc.SetConcurrentConsumerLimit(5); //No more than 5 threads at a time

                //This is how you subscribe to messages
                sbc.Subscribe(subs => subs.Consumer(() => new ChatConsumer(name)).Transient());

                sbc.UseLog4Net();
            });

            Console.WriteLine("Connected as {0}", name);

            var exitRequested = false;
            while (!exitRequested) //Loop over console commands
            {

                var command = Console.ReadLine();
                switch (command)
                {

                    case "exit": //derp
                        exitRequested = true;
                        break;





                    case "who": //Asks Gets the names of everyone on the chat
                        
                        var whosThereRequest = new WhosThereRequest
                        {
                            Name = name
                        };

                        serviceBus.PublishRequest(whosThereRequest, x =>
                        {
                            x.Handle<WhosThereResponse>(resp =>
                            {
                                if (!name.Equals(resp.Name))
                                {
                                    Console.WriteLine("{0} is here", resp.Name);
                                }
                            });
                        });
                        break;






                    default: //General chat message
                        ClearCurrentConsoleLine(); //Looks prettier
                        var chatMessage = new ChatMessage
                        {
                            From = name,
                            Message = command
                        };
                        serviceBus.Publish(chatMessage);
                        break;
                }
            }

            serviceBus.Dispose();
        }












































        private static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop--;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor - 1);
        }
    }
}

using MassTransit;
using MassTransit.Log4NetIntegration;
using MassTransitChat.Messages;
using System;
using System.Threading.Tasks;

namespace MassTransitChat
{
    class Program
    {
        private const string QueueNameFormat = "chat_{0}";
        static void Main(string[] args)
        {
            Console.WriteLine("What is your name?");
            var name = Console.ReadLine()?.Replace(" ", "_");

            Console.Clear();

            Console.WriteLine("Connecting to chat...");

            /*
             *  This creates a new IBusControl instance
             *  The IBusControl gets your code connected to the routing fabric
             *  and allows you to publish and consume messages
             */
            var serviceBus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                //This is how you specify your address
                //This is also how you configure message consumption strategies
                //While the rules are implcit, they are powerful
                //Two services running at the same 'name' compete for messages
                //Services running on different names each get a copy of the message
                sbc.ReceiveEndpoint(host, string.Format(QueueNameFormat, name), ep =>
                {
                    //This is how you subscribe to messages
                    ep.Consumer(() => new ChatConsumer(name));
                });

                sbc.UseConcurrencyLimit(5); //No more than 5 threads at a time
                sbc.UseLog4Net();
            });
            serviceBus.Start();
            Console.WriteLine("Connected as {0}", name);

            var exitRequested = false;
            while (!exitRequested) //Loop over console commands
            {
                var command = Console.ReadLine();
                switch (command)
                {
                    case "exit": //derp
                        exitRequested = true;
                        var playerLeftChatMessage = new PlayerLeftChat
                        {
                            Name = name
                        };
                        serviceBus.Publish(playerLeftChatMessage);
                        break;
                    case "who": //Asks Gets the names of everyone on the chat
                        
                        var whosThereRequest = new WhosThereRequest
                        {
                            Name = name
                        };

                        serviceBus.PublishRequest(whosThereRequest, x =>
                        {
                            x.Handle<WhosThereResponse>(context => Task.Run(() =>
                            {
                                var nameFromMessage = context.Message.Name;
                                if (!name.Equals(nameFromMessage))
                                {
                                    Console.WriteLine("{0} is here", nameFromMessage);
                                }
                            }));
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

            serviceBus.Stop();
        }

        private static void ClearCurrentConsoleLine()
        {
            var currentLineCursor = Console.CursorTop--;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (var i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, currentLineCursor - 1);
        }
    }
}
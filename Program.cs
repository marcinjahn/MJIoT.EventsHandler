using MjIot.EventsHandler.Models;
using System;

namespace MjIot.EventsHandler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Events Handler");

            string newMessage = @"{DeviceId: ""7"",
                                PropertyName: ""SimulatedSwitchState"",
                                PropertyValue: ""false""
                                }";

            var message = new PropertyDataMessage(newMessage);
            var handler = new EventHandler(message);
            handler.HandleMessage().Wait();
        }
    }
}
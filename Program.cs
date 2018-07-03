using MjIot.EventsHandler.Models;
using MjIot.EventsHandler.Services;
using MJIot.Storage.Models;
using System;

namespace MjIot.EventsHandler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Events Handler");

            string newMessage = @"{DeviceId: ""7"",
                                PropertyName: ""Switch State"",
                                PropertyValue: ""false""
                                }";

            var message = new IncomingMessage(newMessage);
            var handler = new EventHandler(message, new UnitOfWork(), new IoTHubService(), null);
            handler.HandleMessage().Wait();
        }
    }
}
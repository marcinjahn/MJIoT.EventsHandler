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

            string newMessage = @"{DeviceId: ""41"",
                                PropertyName: ""Potentiometer Value"",
                                PropertyValue: ""1024""
                                }";

            var message = new IncomingMessage(newMessage);
            var handler = new EventHandler(message, new UnitOfWork(), new IoTHubService(), null);
            handler.HandleMessage().Wait();
        }
    }
}
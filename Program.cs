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

            string newMessage = @"{""DeviceId"":""45"",""PropertyName"":""Output"",""PropertyValue"":""True""}";

            var message = new IncomingMessage(newMessage);
            var handler = new EventHandler(message, new UnitOfWork(), new IoTHubService(), new ConsoleLogger());
            handler.HandleMessage().Wait();

            Console.ReadLine();
        }
    }

    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
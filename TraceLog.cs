using System;

namespace MjIot.EventsHandler
{
    internal class TraceLog
    {
        public void Info(object message)
        {
            Console.WriteLine(message);
        }
    }
}
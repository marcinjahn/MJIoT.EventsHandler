using Newtonsoft.Json;
using System;

namespace MjIot.EventsHandler.Models
{
    public class IncomingMessage
    {
        public IncomingMessage()
        {
        }

        public IncomingMessage(string message)
        {
            IncomingMessage msg;
            try
            {
                msg = JsonConvert.DeserializeObject<IncomingMessage>(message as string);
            }
            catch(Exception e)
            {
                throw new Exception($"Exception thrown while deserializing string message. Details: {e.Message}");
            }

            DeviceId = msg.DeviceId;
            PropertyName = msg.PropertyName;
            PropertyValue = msg.PropertyValue;
        }

        public IncomingMessage(int deviceId, string propertyName, string propertyValue)
        {
            if (propertyName == null || propertyValue == null)
                throw new ArgumentNullException($"ArgumentNullException thrown while creating PropertyDataMessage object.");

            DeviceId = deviceId;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }

        public int DeviceId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        //public string Timestamp { get; set; }
    }
}
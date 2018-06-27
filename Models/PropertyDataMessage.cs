using Newtonsoft.Json;


namespace MjIot.EventsHandler.Models
{
    public class PropertyDataMessage
    {
        public PropertyDataMessage()
        {

        }

        public PropertyDataMessage(string message)
        {
            var msg = JsonConvert.DeserializeObject<PropertyDataMessage>(message as string);
            DeviceId = msg.DeviceId;
            PropertyName = msg.PropertyName;
            PropertyValue = msg.PropertyValue;
        }

        public PropertyDataMessage(dynamic data)
        {
            DeviceId = data.DeviceId;
            PropertyName = data.PropertyName;
            PropertyValue = data.Value;
            //Timestamp = data.Timestamp;
        }

        public int DeviceId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        //public string Timestamp { get; set; }
    }
}

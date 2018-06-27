namespace MjIot.EventsHandler.Models
{
    public class IotMessage
    {
        public IotMessage(string receiverId, string propertyName, string value)
        {
            ReceiverId = receiverId;
            PropertyName = propertyName;
            PropertyValue = value;
        }

        public string ReceiverId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }
}
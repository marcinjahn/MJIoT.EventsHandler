using System;
using System.Collections.Generic;

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

        public override bool Equals(object other)
        {
            if (other == null)
                return false;

            IotMessage castedOther;
            try
            {
                castedOther = (IotMessage)other;
            }
            catch (Exception)
            {
                return false;
            }

            if (castedOther?.ReceiverId == ReceiverId && 
                castedOther?.PropertyName == PropertyName && 
                castedOther?.PropertyValue == PropertyValue)
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 1630569369;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ReceiverId);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropertyName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropertyValue);
            return hashCode;
        }
    }
}
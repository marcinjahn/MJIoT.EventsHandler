using System;
using MjIot.Storage.Models.EF6Db;

namespace MjIot.EventsHandler
{
    public static class MessageValidator
    {
        public static bool IsMessageValid(PropertyDataMessage message, PropertyFormat propertyFormat)
        {
            if (propertyFormat == PropertyFormat.Boolean)
            {
                if (message.PropertyValue == "true" || message.PropertyValue == "false")
                    return true;
            }
            else if (propertyFormat == PropertyFormat.Number)
            {
                if (Double.TryParse(message.PropertyValue, out double result))
                    return true;
            }
            else if (propertyFormat == PropertyFormat.String)
                return true;

            return false;
        }
    }
}

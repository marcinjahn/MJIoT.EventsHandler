using MjIot.Storage.Models.EF6Db;


namespace MjIot.EventsHandler
{
    public class ValueFormatConverter : IValueModifier
    {
        public string Modify(string value, Connection connection)
        {
            if (value == null)
                return null;

            string result = value;

            var senderPropertyFormat = connection.SenderProperty.Format;
            var listenerPropertyFormat = connection.ListenerProperty.Format;

            if (senderPropertyFormat == PropertyFormat.Number)
            {
                if (listenerPropertyFormat == PropertyFormat.Number)
                {
                    return result;
                }
                else if (listenerPropertyFormat == PropertyFormat.String)
                {
                    return result;
                }
                else if (listenerPropertyFormat == PropertyFormat.Boolean)
                {
                    return result;
                }
            }
            else if (senderPropertyFormat == PropertyFormat.String)
            {
                if (listenerPropertyFormat == PropertyFormat.Number)
                {
                    float number = 0;
                    if (float.TryParse(value, out number))
                        return number.ToString();
                    else
                        return null;
                }
                else if (listenerPropertyFormat == PropertyFormat.String)
                {
                    return result;
                }
                else if (listenerPropertyFormat == PropertyFormat.Boolean)
                {
                    if (value.ToLower() == "true" || value.ToLower() == "on")
                        return "true";
                    else if (value.ToLower() == "false" || value.ToLower() == "off")
                        return "false";
                    else
                        return null;
                }
            }
            else if (senderPropertyFormat == PropertyFormat.Boolean)
            {
                if (listenerPropertyFormat == PropertyFormat.Number)
                {
                    if (result == "true")
                        return "1";
                    else if (result == "false")
                        return "0";
                    else
                        return null;
                }
                else if (listenerPropertyFormat == PropertyFormat.String)
                {
                    return result;
                }
                else if (listenerPropertyFormat == PropertyFormat.Boolean)
                {
                    return result;
                }
            }

            return null;
        }
    }
}

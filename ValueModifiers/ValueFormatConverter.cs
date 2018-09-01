using MjIot.Storage.Models.EF6Db;

namespace MjIot.EventsHandler.ValueModifiers
{
    public class ValueFormatConverter : IValueModifier
    {
        public string Modify(string value, Connection connection)
        {
            if (value == null)
                return null;

            var senderPropertyFormat = connection.SenderProperty.Format;
            var listenerPropertyFormat = connection.ListenerProperty.Format;

            if (senderPropertyFormat == PropertyFormat.Number)
            {
                double number = 0;
                if (!double.TryParse(value.Replace(".", ","), out number))
                    throw new System.NotSupportedException($"Cannot convert given number ({value})");

                if (listenerPropertyFormat == PropertyFormat.Number)
                {
                    return number.ToString();
                }
                else if (listenerPropertyFormat == PropertyFormat.String)
                {
                    return number.ToString();
                }
                else if (listenerPropertyFormat == PropertyFormat.Boolean)
                {
                    return number > 0 ? "true" : "false";
                }
            }
            else if (senderPropertyFormat == PropertyFormat.String)
            {
                if (listenerPropertyFormat == PropertyFormat.Number)
                {
                    double number = 0;
                    if (double.TryParse(value.Replace(".", ","), out number))
                        return number.ToString();
                    else
                        throw new System.NotSupportedException($"Cannot convert given string ({value}) to number");
                }
                else if (listenerPropertyFormat == PropertyFormat.String)
                {
                    return value;
                }
                else if (listenerPropertyFormat == PropertyFormat.Boolean)
                {
                    if (value.ToLower() == "true" || value.ToLower() == "on")
                        return "true";
                    else if (value.ToLower() == "false" || value.ToLower() == "off")
                        return "false";
                    else
                        throw new System.NotSupportedException($"Cannot convert given string ({value}) to boolean");
                }
            }
            else if (senderPropertyFormat == PropertyFormat.Boolean)
            {
                if (listenerPropertyFormat == PropertyFormat.Number && float.TryParse(value, out float _))
                {
                    return value;
                }

                if (value.ToLower() != "true" && value.ToLower() != "false")
                    throw new System.NotSupportedException($"Cannot convert given boolean ({value})");

                if (listenerPropertyFormat == PropertyFormat.Number)
                {
                    if (value.ToLower() == "true")
                        return "1";
                    else
                        return "0";
                }
                else if (listenerPropertyFormat == PropertyFormat.String)
                {
                    return value.ToLower();
                }
                else if (listenerPropertyFormat == PropertyFormat.Boolean)
                {
                    return value.ToLower();
                }
            }

            throw new System.NotSupportedException();
        }
    }
}
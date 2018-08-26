using System;

namespace MjIot.EventsHandler.ValueModifiers
{
    public class ValueInfoGenerator
    {
        public ValueInfo GetInfo(string value)
        {
            if (value == null)
                return new ValueInfo(false, false, null, null, null);
            if (IsValueBoolean(value))
                return new ValueInfo(true, false, null, GetBooleanValue(value), value);
            else if (IsValueNumeric(value))
                return new ValueInfo(false, true, GetNumericValue(value), null, value);
            else
                return new ValueInfo(false, false, null, null, value);
        }

        public bool IsValueBoolean(string value)
        {
            if (value.ToLower() == "true" || value.ToLower() == "false")
                return true;
            else
                return false;
        }

        public bool GetBooleanValue(string value)
        {
            if (value.ToLower() == "true")
                return true;
            else if (value.ToLower() == "false")
                return false;
            else
                throw new ArgumentException("Provided value is not boolean", nameof(value));
        }

        public bool IsValueNumeric(string value)
        {
            var result = double.TryParse(value.Replace('.', ','), out double numericValue);
            return result;
        }

        public double GetNumericValue(string value)
        {
            var result = double.TryParse(value.Replace('.', ','), out double numericValue);
            if (result)
                return numericValue;
            else
                throw new InvalidOperationException("Provided value is not numeric!");
        }
    }
}
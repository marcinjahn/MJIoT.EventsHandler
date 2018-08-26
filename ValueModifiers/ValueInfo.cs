namespace MjIot.EventsHandler.ValueModifiers
{
    public class ValueInfo
    {
        public ValueInfo(bool isBoolean, bool isNumeric, double? numericValue, bool? booleanValue, string stringValue)
        {
            IsBoolean = isBoolean;
            IsNumeric = isNumeric;
            NumericValue = numericValue;
            BooleanValue = booleanValue;
            StringValue = stringValue;
        }

        public bool IsBoolean { get; set; }
        public bool IsNumeric { get; set; }
        public double? NumericValue { get; set; }
        public bool? BooleanValue { get; set; }
        public string StringValue { get; set; }
    }
}
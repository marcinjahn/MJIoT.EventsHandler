using MjIot.Storage.Models.EF6Db;


namespace MjIot.EventsHandler.ValueModifiers
{
    public class Calculation : IValueModifier
    {
        public string Modify(string value, Connection connection)
        {
            var calculation = connection.Calculation;
            var calculaionValue = connection.CalculationValue;

            if (value == null || calculation == null) return null;

            ConnectionCalculation? calculationType = calculation as ConnectionCalculation?;

            if (calculationType == ConnectionCalculation.Addition)
            {
                return (float.Parse(value) + float.Parse(calculaionValue)).ToString();
            }
            else if (calculationType == ConnectionCalculation.Subtraction)
            {
                return (float.Parse(value) - float.Parse(calculaionValue)).ToString();
            }
            else if (calculationType == ConnectionCalculation.Product)
            {
                return (float.Parse(value) * float.Parse(calculaionValue)).ToString();
            }
            else if (calculationType == ConnectionCalculation.Division)
            {
                if (calculaionValue == "0")
                    return null;
                return (float.Parse(value) / float.Parse(calculaionValue)).ToString();
            }
            //else if (calculationType == ConnectionCalculation.)
            //{
            //    return (float.Parse(value) - float.Parse(modifierValue)).ToString();
            //}
            else if (calculationType == ConnectionCalculation.BooleanAnd)
            {
                if (value == "false" || calculaionValue == "false")
                    return "false";
                else
                    return "true";
            }
            else if (calculationType == ConnectionCalculation.BooleanNot)
            {
                return (value == "false") ? "true" : "false";
            }
            else if (calculationType == ConnectionCalculation.BooleanOr)
            {
                if (value == "true" || calculaionValue == "true")
                    return "true";
                else
                    return "false";
            }

            return value;
        }
    }
}

using MjIot.Storage.Models.EF6Db;

namespace MjIot.EventsHandler.ValueModifiers
{
    public class Calculation : IValueModifier
    {
        public string Modify(string value, Connection connection)
        {
            var calculation = connection.Calculation;
            var calculationValue = connection.CalculationValue;

            if (value == null) return null;

            if (calculation == ConnectionCalculation.None)
                return value;

            double numericValue;
            var isValueNumeric = double.TryParse(value.Replace('.', ','), out numericValue);
            double numericCalculationValue;
            var isCalculationValueNumeric = double.TryParse(calculationValue?.Replace('.', ','), out numericCalculationValue);

            if (isValueNumeric)
            {
                if ((calculation == ConnectionCalculation.Addition ||
                    calculation == ConnectionCalculation.Subtraction ||
                    calculation == ConnectionCalculation.Product ||
                    calculation == ConnectionCalculation.Division) && !isCalculationValueNumeric)
                    throw new System.NotSupportedException("Provided value is not numeric, but numeric calculation was requested");

                if (calculation == ConnectionCalculation.Addition)
                {
                    return (numericValue + numericCalculationValue).ToString();
                }
                else if (calculation == ConnectionCalculation.Subtraction)
                {
                    return (numericValue - numericCalculationValue).ToString();
                }
                else if (calculation == ConnectionCalculation.Product)
                {
                    return (numericValue * numericCalculationValue).ToString();
                }
                else if (calculation == ConnectionCalculation.Division)
                {
                    if (numericCalculationValue == 0)
                        throw new System.NotSupportedException("Division by 0 requested.");
                    return (numericValue / numericCalculationValue).ToString();
                }
            }
            
            if (value == "true" || value == "false")
            {
                if (calculation != ConnectionCalculation.BooleanNot && calculationValue != "true" && calculationValue != "false")
                    throw new System.NotSupportedException("Boolean calculation cannot be done with provided calculationValue");

                if (calculation == ConnectionCalculation.BooleanNot)
                {
                    return (value == "false") ? "true" : "false";
                }
                else if (calculation == ConnectionCalculation.BooleanAnd)
                {
                    if (value == "false" || calculationValue == "false")
                        return "false";
                    else
                        return "true";
                }

                else if (calculation == ConnectionCalculation.BooleanOr)
                {
                    if (value == "true" || calculationValue == "true")
                        return "true";
                    else
                        return "false";
                }
            }

            throw new System.NotSupportedException();
        }
    }
}
using MjIot.Storage.Models.EF6Db;
using System;

namespace MjIot.EventsHandler.ValueModifiers
{
    public class Calculation : IValueModifier
    {
        ValueInfoGenerator _generator;

        public Calculation()
        {
            _generator = new ValueInfoGenerator();
        }

        private void ThrowExceptionIfNotNumeric(ValueInfo valueInfo)
        {
            if (!valueInfo.IsNumeric)
                throw new NotSupportedException("Provided value is not numeric, but numeric calculation was requested");
        }

        private void ThrowExceptionIfNotBoolean(ValueInfo valueInfo)
        {
            if (!valueInfo.IsBoolean)
                throw new NotSupportedException("Provided value is not boolean, but boolean calculation was requested");
        }

        public string Modify(string value, Connection connection)
        {
            var calculationType = connection.Calculation;
            var calculationValue = connection.CalculationValue;

            if (value == null) return null;

            if (calculationType == ConnectionCalculation.None)
                return value;

            var valueInfo = _generator.GetInfo(value);
            var calculationValueInfo = _generator.GetInfo(calculationValue);



            if (calculationType == ConnectionCalculation.Addition)
            {
                ThrowExceptionIfNotNumeric(valueInfo);
                ThrowExceptionIfNotNumeric(calculationValueInfo);
                return (valueInfo.NumericValue + calculationValueInfo.NumericValue).ToString();
            }
            if (calculationType == ConnectionCalculation.Subtraction)
            {
                ThrowExceptionIfNotNumeric(valueInfo);
                ThrowExceptionIfNotNumeric(calculationValueInfo);
                return (valueInfo.NumericValue - calculationValueInfo.NumericValue).ToString();
            }
            if (calculationType == ConnectionCalculation.Product)
            {
                ThrowExceptionIfNotNumeric(valueInfo);
                ThrowExceptionIfNotNumeric(calculationValueInfo);
                return (valueInfo.NumericValue * calculationValueInfo.NumericValue).ToString();
            }
            if (calculationType == ConnectionCalculation.Division)
            {
                ThrowExceptionIfNotNumeric(valueInfo);
                ThrowExceptionIfNotNumeric(calculationValueInfo);
                if (calculationValueInfo.NumericValue == 0)
                    throw new NotSupportedException("Division by 0 requested.");
                return (valueInfo.NumericValue / calculationValueInfo.NumericValue).ToString();
            }

            if (calculationType == ConnectionCalculation.BooleanNot)
            {
                ThrowExceptionIfNotBoolean(valueInfo);
                return (valueInfo.BooleanValue.Value == true) ? "false" : "true";
            }
            if (calculationType == ConnectionCalculation.BooleanAnd)
            {
                ThrowExceptionIfNotBoolean(valueInfo);
                ThrowExceptionIfNotBoolean(calculationValueInfo);
                if (valueInfo.BooleanValue.Value == false || calculationValueInfo.BooleanValue.Value == false)
                    return "false";
                else
                    return "true";
            }
            if (calculationType == ConnectionCalculation.BooleanOr)
            {
                ThrowExceptionIfNotBoolean(valueInfo);
                ThrowExceptionIfNotBoolean(calculationValueInfo);
                if (valueInfo.BooleanValue.Value == true || calculationValueInfo.BooleanValue.Value == true)
                    return "true";
                else
                    return "false";
            }

            if (calculationType == ConnectionCalculation.Custom)
            {
                var executor = new CustomCalculationExecutor();
                try
                {
                    var task = executor.CalculateAsync(valueInfo, calculationValueInfo.StringValue);
                    task.Wait();

                    return task.Result;
                }
                catch(Exception e)
                {
                    throw new NotSupportedException("Custom calculation threw an exception", e);
                }

            }

            throw new System.NotSupportedException();
        }
    }
}
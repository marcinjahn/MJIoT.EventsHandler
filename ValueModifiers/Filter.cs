using MjIot.Storage.Models.EF6Db;

namespace MjIot.EventsHandler.ValueModifiers
{
    public class Filter : IValueModifier
    {
        public string Modify(string value, Connection connection)
        {
            var filter = connection.Filter;
            var filterValue = connection.FilterValue;

            if (value == null || filter == null) return null;

            ConnectionFilter? filterType = filter as ConnectionFilter?;

            if (filterType == ConnectionFilter.None)
            {
                return value;
            }
            else if (filterType == ConnectionFilter.Equal)
            {
                if (value.Equals(filterValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.NotEqual)
            {
                if (!value.Equals(filterValue))
                    return value;
                else
                    return null;
            }
            else
            {
                double numericValue;
                var isValueNumeric = double.TryParse(value.Replace('.', ','), out numericValue);
                double numericFilterValue;
                var isFilterValueNumeric = double.TryParse(filterValue?.Replace('.', ','), out numericFilterValue);

                if (!isValueNumeric || !isFilterValueNumeric)
                    throw new System.NotSupportedException("Provided value is not numeric, but numeric filter was requested");

                if (filterType == ConnectionFilter.Greater)
                {
                    if (numericValue > numericFilterValue)
                        return value;
                    else
                        return null;
                }
                else if (filterType == ConnectionFilter.GreaterOrEqual)
                {
                    if (numericValue >= numericFilterValue)
                        return value;
                    else
                        return null;
                }
                else if (filterType == ConnectionFilter.Less)
                {
                    if (numericValue < numericFilterValue)
                        return value;
                    else
                        return null;
                }
                else if (filterType == ConnectionFilter.LessOrEqual)
                {
                    if (numericValue <= numericFilterValue)
                        return value;
                    else
                        return null;
                }
            }

            throw new System.NotSupportedException();
        }
    }
}
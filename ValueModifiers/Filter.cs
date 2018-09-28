﻿using MjIot.Storage.Models.EF6Db;
using System.Globalization;
using System.Threading;

namespace MjIot.EventsHandler.ValueModifiers
{
    public class Filter : IValueModifier
    {
        public Filter()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }
        public string Modify(string value, Connection connection)
        {
            var filterType = connection.Filter;
            var filterValue = connection.FilterValue;

            if (value == null) return null;

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
                var isValueNumeric = double.TryParse(value.Replace(',', '.'), out numericValue);
                double numericFilterValue;
                var isFilterValueNumeric = double.TryParse(filterValue?.Replace(',', '.'), out numericFilterValue);

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
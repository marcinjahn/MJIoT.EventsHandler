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

            if (filterType == ConnectionFilter.Equal)
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
            else if (filterType == ConnectionFilter.Greater)
            {
                if (float.Parse(value) > float.Parse(filterValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.GreaterOrEqual)
            {
                if (float.Parse(value) >= float.Parse(filterValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.Less)
            {
                if (float.Parse(value) < float.Parse(filterValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.LessOrEqual)
            {
                if (float.Parse(value) <= float.Parse(filterValue))
                    return value;
                else
                    return null;
            }
            //else if (filterType == ConnectionFilter.None)
            //{
            //    return value;
            //}

            return value;
        }
    }
}
using MjIot.Storage.Models.EF6Db;

namespace MjIot.EventsHandler
{
    public interface IValueModifier
    {
        string Modify(string value, Connection connection);
    }
}

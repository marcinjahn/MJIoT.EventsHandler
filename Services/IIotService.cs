using System;
using System.Threading.Tasks;
using MjIot.EventsHandler.Models;

namespace MjIot.EventsHandler.Services
{
    public interface IIotService
    {
        Task SendToListenerAsync(IotMessage message);
        Task<Boolean> IsDeviceOnline(string deviceId);
    }
}

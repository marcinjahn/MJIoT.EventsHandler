using MjIot.EventsHandler.Models;
using System;
using System.Threading.Tasks;

namespace MjIot.EventsHandler.Services
{
    public interface IIotService
    {
        Task SendToListenerAsync(IotMessage message);

        Task<Boolean> IsDeviceOnline(string deviceId);
    }
}
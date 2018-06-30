using MjIot.EventsHandler.Models;
using System;
using System.Threading.Tasks;

namespace MjIot.EventsHandler.Services
{
    public interface IIotService
    {
        Task SendToListenerAsync(MessageForListener message);

        Task<Boolean> IsDeviceOnline(string deviceId);
    }
}
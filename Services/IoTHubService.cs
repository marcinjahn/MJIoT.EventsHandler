using System;
using System.Threading.Tasks;

using Microsoft.Azure.Devices;
using MjIot.EventsHandler.Models;

namespace MjIot.EventsHandler.Services
{

    public class IoTHubService : IIotService
    {
        public ServiceClient ServiceClient { get; set; }
        public string ConnectionString { get; set; }

        public IoTHubService()
        {
            ConnectionString = "HostName=MJIoT-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SzQKdF1y6bAEgGfZei2bmq1Jd83odc+B2x197n2MtxA=";
            ServiceClient = Microsoft.Azure.Devices.ServiceClient.CreateFromConnectionString(ConnectionString);
        }

        public async Task SendToListenerAsync(IotMessage message)
        {
            var messageString = GenerateC2DMessage(message.PropertyName, message.PropertyValue);
            await SendC2DMessageAsync(message.ReceiverId, messageString);
        }

        public async Task<Boolean> IsDeviceOnline(string deviceId)
        {
            var methodInvocation = new CloudToDeviceMethod("conn") { ResponseTimeout = TimeSpan.FromSeconds(5) };
            CloudToDeviceMethodResult response;
            try
            {
                response = await ServiceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private async Task SendC2DMessageAsync(string deviceId, string message)
        {
            var messageObject = new Message(System.Text.Encoding.ASCII.GetBytes(message));
            await ServiceClient.SendAsync(deviceId, messageObject);
        }

        private string GenerateC2DMessage(string property, string value)
        {
            return @"{""PropertyName"":""" + property + @""",""PropertyValue"":""" + value + @"""}";
        }

    }
}

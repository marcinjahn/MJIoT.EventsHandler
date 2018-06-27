using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MjIot.Storage.Models.EF6Db;
using MJIot.Storage.Models;
using System.Data.Entity;
using System.Linq;
using MjIot.EventsHandler.Models;
using MjIot.EventsHandler.Services;
using MjIot.EventsHandler.ValueModifiers;

namespace MjIot.EventsHandler
{

    public class EventHandler
    {
        private IoTHubService _iotHubService { get; set; }
        private PropertyDataMessage _message { get; set; }
        private IUnitOfWork _unitOfWork;

        private Device _senderDevice;
        private PropertyType _senderProperty;

        private List<IValueModifier> _valueModifiers;


        public EventHandler(PropertyDataMessage message)
        {
            _unitOfWork = new UnitOfWork();
            _message = message;

            _senderDevice = _unitOfWork.Devices.Get(_message.DeviceId);
            if (_senderDevice == null)
                throw new Exception("Sender device not found");

            _senderProperty = _unitOfWork.PropertyTypes.GetPropertiesOfDevice(_senderDevice.DeviceType)
                .Where(n => n.Name == _message.PropertyName)
                .FirstOrDefault();
            if (_senderProperty == null)
                throw new Exception("Sender property not found");

            _iotHubService = new IoTHubService();
            _valueModifiers = new List<IValueModifier> { new Filter(), new Calculation(), new ValueFormatConverter() };
        }

        public async Task HandleMessage()
        {
            var isMessageValid = MessageValidator.IsMessageValid(_message, _senderProperty.Format);
            if (!isMessageValid)
                throw new Exception($"Received value does not match the declared type of sender property! (DeviceId: {_message.DeviceId}, Property: {_message.PropertyName}, Value: {_message.PropertyValue})");

            if (_senderProperty.IsSenderProperty)
                await NotifyListeners();
        }

        private async Task NotifyListeners()
        {
            var connections = _unitOfWork.Connections.GetDeviceConnections(_senderDevice);

            var notifyTasks = new List<Task>();

            foreach (var connection in connections)
                notifyTasks.Add(NotifyListener(connection));

            await Task.WhenAll(notifyTasks);
        }

        private async Task NotifyListener(Connection connection)
        {
            var deviceType = connection.ListenerDevice.DeviceType;

            if (await CheckDeviceOfflineAvailability(deviceType, connection.ListenerDevice.Id))
            {
                var message = GetMessageToSend(connection);
                await _iotHubService.SendToListenerAsync(message);
            }
        }

        private async Task<bool> CheckDeviceOfflineAvailability(DeviceType listenerDeviceType, int listenerId)
        {
            if (!listenerDeviceType.OfflineMessagesEnabled)
            {
                if (!(await _iotHubService.IsDeviceOnline(listenerId.ToString())))
                    return false;
            }

            return true;
        }

        private IotMessage GetMessageToSend(Connection connection)
        {
            var valueToSend = GetFinalValueToSend(connection, connection.ListenerProperty.Format);
            if (valueToSend == null)
                throw new ArgumentNullException("Sending was blocked by a filter.");

            return new IotMessage(connection.ListenerDevice.Id.ToString(), connection.ListenerProperty.Name, valueToSend);
        }

        private string GetFinalValueToSend(Connection connection, PropertyFormat listenerPropertyFormat)
        {
            string modifiedValue = _message.PropertyValue;
            foreach (var modifer in _valueModifiers)
                modifiedValue = modifer.Modify(modifiedValue, connection);

            return modifiedValue;
        }
    }
}

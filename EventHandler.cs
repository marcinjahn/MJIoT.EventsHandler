﻿using MjIot.EventsHandler.Models;
using MjIot.EventsHandler.Services;
using MjIot.EventsHandler.ValueModifiers;
using MjIot.Storage.Models.EF6Db;
using MJIot.Storage.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MjIot.EventsHandler
{
    public class EventHandler
    {
        private IIotService _iotService { get; set; }
        private PropertyDataMessage _message { get; set; }
        private IUnitOfWork _unitOfWork;
        ILogger _logger;

        private Device _senderDevice;
        private PropertyType _senderProperty;

        private List<IValueModifier> _valueModifiers;

        public EventHandler(PropertyDataMessage message, IUnitOfWork unitOfWork, IIotService iotService, ILogger logger)
        {
            Log("Event handler creation started");

            if (message == null || unitOfWork == null || iotService == null)
                throw new ArgumentNullException("EventHandler couldn't be instantiated, because given arguments were NULL.");

            _logger = logger;
            _unitOfWork = unitOfWork;
            _message = message;

            _senderDevice = _unitOfWork.Devices.Get(_message.DeviceId);
            if (_senderDevice == null)
                throw new Exception("Sender device not found");

            _senderProperty = _unitOfWork.PropertyTypes.GetPropertiesOfDevice(_senderDevice.DeviceType)
                .Where(n => n.Name == _message.PropertyName)
                .FirstOrDefault();
            if (_senderProperty == null)
                throw new Exception("Sender property not found");

            _iotService = iotService;
            _valueModifiers = new List<IValueModifier> { new Filter(), new Calculation(), new ValueFormatConverter() };

            Log("Event handler successfully created");
        }

        public async Task<bool> HandleMessage()
        {
            var isMessageValid = MessageValidator.IsMessageValid(_message, _senderProperty.Format);
            if (!isMessageValid)
            {
                Log("Message was not valid. Terminating.");
                throw new Exception($"Received value does not match the declared type of sender property! (DeviceId: {_message.DeviceId }, Property: {_message.PropertyName }, Value: {_message.PropertyValue })");
            }
                
            if (_senderProperty.IsSenderProperty)
            {
                Log("Sender property. Notifying listeners");
                await NotifyListeners();
                return true;
            }
            else
            {
                Log("Not a sender property. Execution finished");
                return false;
            }


        }

        private async Task NotifyListeners()
        {
            var connections = _unitOfWork.Connections.GetDeviceConnections(_senderDevice);

            var notifyTasks = new List<Task>();

            foreach (var connection in connections)
                notifyTasks.Add(NotifyListener(connection));

            if (notifyTasks.Count() == 0)
                Log("There are no listeners. Execution finished");

            await Task.WhenAll(notifyTasks);
        }

        private async Task NotifyListener(Connection connection)
        {
            var deviceType = connection.ListenerDevice.DeviceType;

            if (await CheckDeviceOfflineAvailability(deviceType, connection.ListenerDevice.Id))
            {
                Log($"Online check passed for listener of ID = {connection.ListenerDevice.Id}. Message will be sent.");
                var message = GetMessageToSend(connection);
                await _iotService.SendToListenerAsync(message);
            }
            else
                Log($"Online check failed for listener of ID = {connection.ListenerDevice.Id}. Message will not be sent");
        }

        private async Task<bool> CheckDeviceOfflineAvailability(DeviceType listenerDeviceType, int listenerId)
        {
            if (!listenerDeviceType.OfflineMessagesEnabled)
            {
                if (!(await _iotService.IsDeviceOnline(listenerId.ToString())))
                    return false;
            }

            return true;
        }

        private IotMessage GetMessageToSend(Connection connection)
        {
            var valueToSend = GetFinalValueToSend(connection, connection.ListenerProperty.Format);
            if (valueToSend == null)
                throw new ArgumentNullException("Sending was blocked because value is null.");

            return new IotMessage(connection.ListenerDevice.Id.ToString(), connection.ListenerProperty.Name, valueToSend);
        }

        private string GetFinalValueToSend(Connection connection, PropertyFormat listenerPropertyFormat)
        {
            string modifiedValue = _message.PropertyValue;
            foreach (var modifer in _valueModifiers)
                modifiedValue = modifer.Modify(modifiedValue, connection);

            return modifiedValue;
        }

        private void Log(string message)
        {
            if (_logger != null)
                _logger.Log(message);
        }

    }

    public interface ILogger
    {
        void Log(string message);
    }
}
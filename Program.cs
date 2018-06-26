﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MjIot.Storage.Models.EF6Db;
using Newtonsoft.Json;

using System.Data.Entity;

using Microsoft.Azure.Devices;


namespace MjIot.EventsHandler
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Events Handler");

            string newMessage = @"{DeviceId: ""7"",
                                PropertyName: ""SimulatedSwitchState"",
                                PropertyValue: ""false""
                                }";

            var message = new PropertyDataMessage(newMessage);
            var handler = new EventHandler(message);
            handler.HandleMessage().Wait();
        }
    }

    public class EventHandler
    {
        private IoTHubService IoTHubService { get; set; }
        private MJIoTDb ModelDb { get; set; }
        private PropertyDataMessage Message { get; set; }

        public EventHandler(PropertyDataMessage message)
        {
            IoTHubService = new IoTHubService();
            ModelDb = new MJIoTDb();
            Message = message;
        }

        public async Task HandleMessage()
        {
            if (!IsMessageValid(Message))
                throw new Exception($"Received value does not match the declared type of sender property! (DeviceId: {Message.DeviceId}, Property: {Message.PropertyName}, Value: {Message.PropertyValue})");


            var isSenderProperty = ModelDb.IsItSenderProperty(Message.DeviceId, Message.PropertyName);
            if (isSenderProperty)
                await NotifyListeners();

        }

        public bool IsMessageValid(PropertyDataMessage message)
        {
            var propertyFormat = ModelDb.GetPropertyType(message.DeviceId, message.PropertyName).Format;

            if (propertyFormat == PropertyFormat.Boolean)
            {
                if (message.PropertyValue == "true" || message.PropertyValue == "false")
                    return true;
            }
            else if (propertyFormat == PropertyFormat.Number)
            {
                if (Double.TryParse(message.PropertyValue, out double result))
                    return true;
            }
            else if (propertyFormat == PropertyFormat.String)
                return true;

            return false;
        }

        private async Task NotifyListeners()
        {
            var connections = ModelDb.GetConnections(Message.DeviceId);
            var notifyTasks = new List<Task>();
            foreach (var connection in connections)
                notifyTasks.Add(NotifyListener(connection));
            await Task.WhenAll(notifyTasks);
        }

        private async Task NotifyListener(Connection connection)
        {
            var deviceType = connection.ListenerDevice.DeviceType;

            if (await ShouldMessageBeSent(deviceType, connection.ListenerDevice.Id))
            {
                var message = GetMessageToSend(connection);
                await IoTHubService.SendToListenerAsync(message);
            }
        }

        private async Task<bool> ShouldMessageBeSent(DeviceType listenerDeviceType, int listenerId)
        {
            //OFFLINE MESSAGING ENABLED?
            if (!ModelDb.IsOfflineMessagingEnabled(listenerDeviceType))
            {
                //IS DEVICE ONLINE?
                if (!(await IoTHubService.IsDeviceOnline(listenerId.ToString())))
                    return false;
            }

            return true;
        }

        private IoTHubMessage GetMessageToSend(Connection connection)
        {
            var senderPropertyType = ModelDb.GetPropertyType(connection.SenderDevice.Id, connection.SenderProperty.Name);
            var listenerPropertyType = ModelDb.GetPropertyType(connection.ListenerDevice.Id, connection.ListenerProperty.Name);
            //var convertedValue =  MessageConverter.Convert(Message.PropertyValue, format);
            var convertedValue = ValueConverter.GetMessageValue(senderPropertyType.Format, listenerPropertyType.Format, Message.PropertyValue, connection);
            if (convertedValue == null)
                throw new ArgumentNullException("Sending was blocked by a filter.");
            return new IoTHubMessage(connection.ListenerDevice.Id.ToString(), listenerPropertyType.Name, convertedValue);
        }
    }

    public class IoTHubMessage
    {
        public IoTHubMessage(string receiverId, string propertyName,  string value)
        {
            ReceiverId = receiverId;
            PropertyName = propertyName;
            PropertyValue = value;
        }

        public string ReceiverId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }


    public class MJIoTDb
    {
        public MJIoTDBContext Context { get; set; }

        public MJIoTDb()
        {
            Context = new MJIoTDBContext();
        }

        public void SaveValue(PropertyDataMessage message)
        {
            DeviceProperty deviceProperty = GetDeviceProperty(message);
            deviceProperty.PropertyValue = message.PropertyValue;
            Context.SaveChanges();
        }

        public List<Connection> GetConnections(int senderId)
        {
            //var listeners = Context.Devices.Include("ListenerDevices")
            //        .Where(n => n.Id == senderId)
            //        .Select(n => n.ListenerDevices)
            //        .FirstOrDefault();

            var connections = Context.Connections
                    .Include(n => n.ListenerDevice.DeviceType)
                    .Include(n => n.SenderDevice)
                    .Include(n => n.SenderProperty)
                    .Include(n => n.ListenerProperty)
                .Where(n => n.SenderDevice.Id == senderId)
                .ToList();

            return connections;
        }

        public bool IsItSenderProperty(int deviceId, string property)
        {
            //var deviceType = GetDeviceType(deviceId);
            //if (deviceType.SenderProperty == null)
            //    return false;
            //else
            //    return deviceType.SenderProperty.Name == property;

            if (GetPropertyType(deviceId, property).IsSenderProperty)
                return true;
            else
                return false;

        }

        public PropertyType GetPropertyType(int deviceId, string property)
        {
            var deviceType = GetDeviceType(deviceId);

            var propertyType = Context.PropertyTypes
                .Where(n => n.DeviceType.Id == deviceType.Id && n.Name == property)
                .ToList()
                .FirstOrDefault();

            if (propertyType == null)
            {
                throw new Exception("GetPropertyType didn't find the Property Type!!");
            }

            return propertyType;
        }

        //public PropertyType GetListenerPropertyType(DeviceType deviceType) //NEEDS TO BE TESTED
        //{
        //    var i = 0;
        //    while (true && i <= 100)  //DANGEROUS - MIGHT BE INIFNITE (for now <= 100 is a workaround)
        //    {
        //        i++;
        //        var property = deviceType.ListenerProperty;

        //        if (property != null)
        //            return property;
        //        else
        //        {
        //            var baseType = deviceType.BaseDeviceType;

        //            if (baseType != null)
        //                deviceType = baseType;
        //        }
        //    }

        //    throw new Exception("GetListenerPropertyType didn't find the Property Type!!");
        //}

        public bool IsOfflineMessagingEnabled(DeviceType deviceType)
        {
            return deviceType.OfflineMessagesEnabled;
        }

        private DeviceProperty GetDeviceProperty(PropertyDataMessage message)
        {
            DeviceType deviceType = GetDeviceType(message.DeviceId);
            PropertyType propertyType = GetPropertyType(message.PropertyName, deviceType.Id);
            return GetDeviceProperty(message.DeviceId, propertyType.Id);
        }

        private DeviceProperty GetDeviceProperty(int deviceId, int propertyTypeId)
        {
            var deviceProperty = Context.DeviceProperties
                            .Include("PropertyType").Include("Device")
                            .Where(n => n.Device.Id == deviceId && n.PropertyType.Id == propertyTypeId)
                            .FirstOrDefault();

            if (deviceProperty == null)
            {
                throw new NullReferenceException("Property (" + propertyTypeId.ToString() + ") of device (" + deviceId + ") does not exist. Procedure aborted.");
            }

            return deviceProperty;
        }

        private PropertyType GetPropertyType(string propertyName, int deviceTypeId)
        {
            var propertyType = Context.PropertyTypes
                            .Include("DeviceType")
                            .Where(n => n.Name == propertyName && n.DeviceType.Id == deviceTypeId)
                            .FirstOrDefault();
            if (propertyType == null)
            {
                throw new NullReferenceException("Property type (" + propertyName + ") does not exist. Procedure aborted.");
            }

            return propertyType;
        }

        public DeviceType GetDeviceType(int deviceId)
        {

            //var deviceType = Context.Devices.Include("DeviceType").Include("DeviceType.SenderProperty").Include("DeviceType.ListenerProperty")
            //        .Where(n => n.Id == deviceId)
            //        .Select(n => n.DeviceType)
            //        .FirstOrDefault();


            //lambda in Include() requires using System.Data.Entity;
            var deviceType = Context.Devices.Include(n => n.DeviceType)
                   .Where(n => n.Id == deviceId)
                   .Select(n => n.DeviceType)
                   .FirstOrDefault();

            if (deviceType == null)
            {
                throw new NullReferenceException("Device (" + deviceId + ") does not exist. Procedure aborted.");
            }

            return deviceType;
        }
    }


    public class IoTHubService
    {
        public ServiceClient ServiceClient { get; set; }
        public string ConnectionString { get; set; }

        public IoTHubService()
        {
            ConnectionString = "HostName=MJIoT-Hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=SzQKdF1y6bAEgGfZei2bmq1Jd83odc+B2x197n2MtxA=";
            ServiceClient = Microsoft.Azure.Devices.ServiceClient.CreateFromConnectionString(ConnectionString);
        }

        public async Task SendToListenerAsync(IoTHubMessage message)
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

    public class PropertyDataMessage
    {
        public PropertyDataMessage()
        {

        }

        public PropertyDataMessage(string message)
        {
            var msg = JsonConvert.DeserializeObject<PropertyDataMessage>(message as string);
            DeviceId = msg.DeviceId;
            PropertyName = msg.PropertyName;
            PropertyValue = msg.PropertyValue;
        }

        public PropertyDataMessage(dynamic data)
        {
            DeviceId = data.DeviceId;
            PropertyName = data.PropertyName;
            PropertyValue = data.Value;
            //Timestamp = data.Timestamp;
        }

        public int DeviceId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        //public string Timestamp { get; set; }
    }



    public class ValueConverter
    {
        public static string GetMessageValue(PropertyFormat senderType, PropertyFormat listenerType, string value, Connection connection)
        {
            string result;
            var filter = new Filter();
            var calculation = new Calculation();

            result = filter.Modify(value, connection.Filter, connection.FilterValue);
            result = calculation.Modify(result, connection.Calculation, connection.CalculationValue);

            if (result == null)
                return null;

            if (senderType == PropertyFormat.Number)
            {
                if (listenerType == PropertyFormat.Number)
                {
                    return result;
                }
                else if (listenerType == PropertyFormat.String)
                {
                    return result;
                }
                else if (listenerType == PropertyFormat.Boolean)
                {
                    return result;
                }
            }
            else if (senderType == PropertyFormat.String)
            {
                if (listenerType == PropertyFormat.Number)
                {
                    float number = 0;
                    if (float.TryParse(value, out number))
                        return number.ToString();
                    else
                        return null;
                }
                else if (listenerType == PropertyFormat.String)
                {
                    return result;
                }
                else if (listenerType == PropertyFormat.Boolean)
                {
                    if (value.ToLower() == "true" || value.ToLower() == "on")
                        return "true";
                    else if (value.ToLower() == "false" || value.ToLower() == "off")
                        return "false";
                    else
                        return null;
                }
            }
            else if (senderType == PropertyFormat.Boolean)
            {
                if (listenerType == PropertyFormat.Number)
                {
                    if (result == "true")
                        return "1";
                    else if (result == "false")
                        return "0";
                    else
                        return null;
                }
                else if (listenerType == PropertyFormat.String)
                {
                    return result;
                }
                else if (listenerType == PropertyFormat.Boolean)
                {
                    return result;
                }
            }

            return null;
        }
    }

    public interface IValueModifier
    {
        string Modify(string value, object modifier, string modifierValue);
    }

    public class Calculation : IValueModifier
    {
        public string Modify(string value, object modifier, string modifierValue)
        {
            if (value == null || modifier == null) return null;

            ConnectionCalculation? calculationType = modifier as ConnectionCalculation?;

            if (calculationType == ConnectionCalculation.Addition)
            {
                return (float.Parse(value) + float.Parse(modifierValue)).ToString();
            }
            else if (calculationType == ConnectionCalculation.Subtraction)
            {
                return (float.Parse(value) - float.Parse(modifierValue)).ToString();
            }
            else if (calculationType == ConnectionCalculation.Product)
            {
                return (float.Parse(value) * float.Parse(modifierValue)).ToString();
            }
            else if (calculationType == ConnectionCalculation.Division)
            {
                if (modifierValue == "0")
                    return null;
                return (float.Parse(value) / float.Parse(modifierValue)).ToString();
            }
            //else if (calculationType == ConnectionCalculation.)
            //{
            //    return (float.Parse(value) - float.Parse(modifierValue)).ToString();
            //}
            else if (calculationType == ConnectionCalculation.BooleanAnd)
            {
                if (value == "false" || modifierValue == "false")
                    return "false";
                else
                    return "true";
            }
            else if (calculationType == ConnectionCalculation.BooleanNot)
            {
                return (value == "false") ? "true" : "false";
            }
            else if (calculationType == ConnectionCalculation.BooleanOr)
            {
                if (value == "true" || modifierValue == "true")
                    return "true";
                else
                    return "false";
            }

            return value;
        }
    }

    public class Filter : IValueModifier
    {
        public string Modify(string value, object modifier, string modifierValue)
        {
            if (value == null || modifier == null) return null;

            ConnectionFilter? filterType = modifier as ConnectionFilter?;

            if (filterType == ConnectionFilter.Equal)
            {
                if (value.Equals(modifierValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.NotEqual)
            {
                if (!value.Equals(modifierValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.Greater)
            {
                if (float.Parse(value) > float.Parse(modifierValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.GreaterOrEqual)
            {
                if (float.Parse(value) >= float.Parse(modifierValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.Less)
            {
                if (float.Parse(value) < float.Parse(modifierValue))
                    return value;
                else
                    return null;
            }
            else if (filterType == ConnectionFilter.LessOrEqual)
            {
                if (float.Parse(value) <= float.Parse(modifierValue))
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



    //public class MessageConverter
    //{
    //    public static string Convert(string value, PropertyTypeFormat targetType)
    //    {
    //        var stringValue = value;
    //        double floatValue;
    //        //int intValue;

    //        //ONEBYTE
    //        //if (targetType == PropertyTypeFormats.OneByte)
    //        //{
    //        //    if (stringValue == "true")
    //        //        return "255";
    //        //    else if (stringValue == "false")
    //        //        return "0";
    //        //    else if (Double.TryParse(stringValue, out floatValue))
    //        //    {
    //        //        intValue = (int)floatValue > 255 ? 255 : (int)floatValue;
    //        //        return intValue.ToString();
    //        //    }
    //        //    else
    //        //        return "0";
    //        //}

    //        //BOOLEAN
    //        else if (targetType == PropertyTypeFormat.Boolean)
    //        {
    //            if (stringValue == "true" || stringValue.ToLower() == "on")
    //                return "true";
    //            else if (stringValue == "false" || stringValue.ToLower() == "off")
    //                return "false";
    //            else if (Double.TryParse(stringValue, out floatValue))
    //            {
    //                if (floatValue > 0)
    //                    return "true";
    //                else
    //                    return "false";
    //            }
    //            else
    //                return "false";
    //        }

    //        //NUMBER
    //        else if (targetType == PropertyTypeFormat.Number)
    //        {
    //            if (stringValue == "true")
    //                return "1";  //?? What value should it be?
    //            else if (stringValue == "false")
    //                return "0";
    //            else if (Double.TryParse(stringValue, out floatValue))
    //            {
    //                return floatValue.ToString();
    //            }
    //            else
    //                return "0";
    //        }

    //        //RAW
    //        else
    //        {
    //            return stringValue;
    //        }
    //    }
    //}
}

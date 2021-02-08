﻿using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using HSMSensorDataObjects;
using HSMServer.DataLayer.Model;
using HSMServer.DataLayer.Model.TypedDataObjects;
using HSMServer.Model;
using NLog;
using SensorsService;
using RSAParameters = System.Security.Cryptography.RSAParameters;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

namespace HSMServer.MonitoringServerCore
{
    public static class Converter
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static SignedCertificateMessage Convert(X509Certificate2 signedCertificate,
            X509Certificate2 caCertificate)
        {
            SignedCertificateMessage message = new SignedCertificateMessage();
            message.CaCertificateBytes = ByteString.CopyFrom(caCertificate.Export(X509ContentType.Cert));
            message.SignedCertificateBytes = ByteString.CopyFrom(signedCertificate.Export(X509ContentType.Pfx));
            return message;
        }

        #region Convert to history items

        public static SensorHistoryMessage Convert(SensorDataObject dataObject)
        {
            SensorHistoryMessage result = new SensorHistoryMessage();
            result.TypedData = ByteString.CopyFrom(Encoding.UTF8.GetBytes(dataObject.TypedData));
            result.Time = Timestamp.FromDateTime(dataObject.TimeCollected);
            result.Type = Convert(dataObject.DataType);
            return result;
        }

        #endregion
        #region Convert to database objects

        private static void FillCommonFields(SensorValueBase value, DateTime timeCollected, out SensorDataObject dataObject)
        {
            dataObject = new SensorDataObject();
            dataObject.Path = value.Path;
            dataObject.Time = value.Time;
            dataObject.TimeCollected = timeCollected;
            dataObject.Timestamp = GetTimestamp(value.Time);
        }

        private static SensorDataTypes Convert(SensorObjectType type)
        {
            switch (type)
            {
                case SensorObjectType.ObjectTypeBoolSensor:
                    return SensorDataTypes.BoolSensor;
                case SensorObjectType.ObjectTypeDoubleSensor:
                    return SensorDataTypes.DoubleSensor;
                case SensorObjectType.ObjectTypeIntSensor:
                    return SensorDataTypes.IntSensor;
                case SensorObjectType.ObjectTypeStringSensor:
                    return SensorDataTypes.StringSensor;
                case SensorObjectType.ObjectTypeBarDoubleSensor:
                    return SensorDataTypes.BarDoubleSensor;
                case SensorObjectType.ObjectTypeBarIntSensor:
                    return SensorDataTypes.BarIntSensor;
                default:
                    throw new InvalidEnumArgumentException($"Invalid SensorDataType: {type}");
            }
        }
        //public static SensorDataObject ConvertToDatabase(SensorUpdateMessage update, DateTime originalTime)
        //{
        //    SensorDataObject result = new SensorDataObject();
        //    result.Path = update.Path;
        //    result.Time = originalTime;
        //    result.TypedData = update.DataObject.ToString(Encoding.UTF8);
        //    result.TimeCollected = update.Time.ToDateTime();
        //    result.Timestamp = GetTimestamp(result.TimeCollected);
        //    result.DataType = Convert(update.ObjectType);
        //    return result;
        //}

        public static SensorDataObject ConvertToDatabase(BoolSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorDataTypes.BoolSensor;

            BoolSensorData typedData = new BoolSensorData() { BoolValue = sensorValue.BoolValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public static SensorDataObject ConvertToDatabase(IntSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorDataTypes.IntSensor;

            IntSensorData typedData = new IntSensorData() { IntValue = sensorValue.IntValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public static SensorDataObject ConvertToDatabase(DoubleSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorDataTypes.DoubleSensor;

            DoubleSensorData typedData = new DoubleSensorData() { DoubleValue = sensorValue.DoubleValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public static SensorDataObject ConvertToDatabase(StringSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorDataTypes.StringSensor;

            StringSensorData typedData = new StringSensorData() { StringValue = sensorValue.StringValue, Comment = sensorValue.Comment };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public static SensorDataObject ConvertToDatabase(IntBarSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorDataTypes.BarIntSensor;

            IntBarSensorData typedData = new IntBarSensorData()
            {
                Max = sensorValue.Max,
                Min = sensorValue.Min,
                Mean = sensorValue.Mean,
                Count = sensorValue.Count,
                Comment = sensorValue.Comment
            };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        public static SensorDataObject ConvertToDatabase(DoubleBarSensorValue sensorValue, DateTime timeCollected)
        {
            SensorDataObject result;
            FillCommonFields(sensorValue, timeCollected, out result);
            result.DataType = SensorDataTypes.BarIntSensor;

            DoubleBarSensorData typedData = new DoubleBarSensorData()
            {
                Max = sensorValue.Max,
                Min = sensorValue.Min,
                Mean = sensorValue.Mean,
                Count = sensorValue.Count,
                Comment = sensorValue.Comment
            };
            result.TypedData = JsonSerializer.Serialize(typedData);
            return result;
        }

        #endregion


        #region Convert to update messages

        public static SensorUpdateMessage Convert(SensorDataObject dataObject, string productName)
        {
            SensorUpdateMessage result = new SensorUpdateMessage();
            result.Path = dataObject.Path;
            result.ObjectType = Convert(dataObject.DataType);
            result.Product = productName;
            result.Time = Timestamp.FromDateTime(dataObject.TimeCollected.ToUniversalTime());
            result.ShortValue = GetShortValue(dataObject.TypedData, dataObject.DataType);
            return result;
        }
        
        public static SensorUpdateMessage Convert(BoolSensorValue value, string productName, DateTime timeCollected)
        {
            SensorUpdateMessage update;
            AddCommonValues(value, productName, timeCollected, out update);
            update.ShortValue = GetShortValue(value);
            update.ObjectType = SensorObjectType.ObjectTypeBoolSensor;
            update.ActionType = SensorUpdateMessage.Types.TransactionType.TransAdd;

            return update;
        }

        public static SensorUpdateMessage Convert(IntSensorValue value, string productName, DateTime timeCollected)
        {
            SensorUpdateMessage update;
            AddCommonValues(value, productName, timeCollected, out update);
            update.ShortValue = GetShortValue(value);
            update.ObjectType = SensorObjectType.ObjectTypeIntSensor;
            update.ActionType = SensorUpdateMessage.Types.TransactionType.TransAdd;

            return update;
        }

        public static SensorUpdateMessage Convert(DoubleSensorValue value, string productName, DateTime timeCollected)
        {
            SensorUpdateMessage update;
            AddCommonValues(value, productName, timeCollected, out update);
            update.ShortValue = GetShortValue(value);
            update.ObjectType = SensorObjectType.ObjectTypeDoubleSensor;
            update.ActionType = SensorUpdateMessage.Types.TransactionType.TransAdd;

            return update;
        }

        public static SensorUpdateMessage Convert(StringSensorValue value, string productName, DateTime timeCollected)
        {
            SensorUpdateMessage update;
            AddCommonValues(value, productName, timeCollected, out update);
            update.ShortValue = GetShortValue(value);
            update.ObjectType = SensorObjectType.ObjectTypeStringSensor;
            update.ActionType = SensorUpdateMessage.Types.TransactionType.TransAdd;

            return update;
        }

        public static SensorUpdateMessage Convert(IntBarSensorValue value, string productName, DateTime timeCollected)
        {
            SensorUpdateMessage update;
            AddCommonValues(value, productName, timeCollected, out update);
            update.ShortValue = GetShortValue(value);
            update.ObjectType = SensorObjectType.ObjectTypeBarIntSensor;
            update.ActionType = SensorUpdateMessage.Types.TransactionType.TransAdd;

            return update;
        }

        public static SensorUpdateMessage Convert(DoubleBarSensorValue value, string productName, DateTime timeCollected)
        {
            SensorUpdateMessage update;
            AddCommonValues(value, productName, timeCollected, out update);
            update.ShortValue = GetShortValue(value);
            update.ObjectType = SensorObjectType.ObjectTypeBarDoubleSensor;
            update.ActionType = SensorUpdateMessage.Types.TransactionType.TransAdd;

            return update;
        }
        private static void AddCommonValues(SensorValueBase value, string productName, DateTime timeCollected, out SensorUpdateMessage update)
        {
            update = new SensorUpdateMessage();
            update.Path = value.Path;
            update.Product = productName;
            update.Time = Timestamp.FromDateTime(timeCollected.ToUniversalTime());
        }

        #endregion

        #region Typed data objects

        private static string GetShortValue(string stringData, SensorDataTypes sensorType)
        {
            switch (sensorType)
            {
                case SensorDataTypes.BoolSensor:
                {
                    BoolSensorData boolData = JsonSerializer.Deserialize<BoolSensorData>(stringData);
                    return $"Value = {boolData.BoolValue}";
                }
                case SensorDataTypes.IntSensor:
                {
                    IntSensorData intData = JsonSerializer.Deserialize<IntSensorData>(stringData);
                    return $"Value = {intData.IntValue}";
                }
                case SensorDataTypes.DoubleSensor:
                {
                    DoubleSensorData doubleData = JsonSerializer.Deserialize<DoubleSensorData>(stringData);
                    return $"Value = {doubleData.DoubleValue}";
                }
                case SensorDataTypes.StringSensor:
                {
                    StringSensorData stringTypedData = JsonSerializer.Deserialize<StringSensorData>(stringData);
                    return $"Value = '{stringTypedData.StringValue}'";
                }
                case SensorDataTypes.BarIntSensor:
                {
                    IntBarSensorData intBarData = JsonSerializer.Deserialize<IntBarSensorData>(stringData);
                    return $"Value: Min = {intBarData.Min}, Mean = {intBarData.Mean}, Max = {intBarData.Max}, Count = {intBarData.Count}";
                }
                case SensorDataTypes.BarDoubleSensor:
                {
                    DoubleBarSensorData doubleBarData = JsonSerializer.Deserialize<DoubleBarSensorData>(stringData);
                    return $"Value: Min = {doubleBarData.Min}, Mean = {doubleBarData.Mean}, Max = {doubleBarData.Max}, Count = {doubleBarData.Count}";
                }
                default:
                    throw new ApplicationException($"Unknown data type: {sensorType}!");
            }
        }
        private static string GetShortValue(BoolSensorValue value)
        {
            return $"Value = {value.BoolValue}";
        }

        private static string GetShortValue(IntSensorValue value)
        {
            return $"Value = {value.IntValue}";
        }

        private static string GetShortValue(DoubleSensorValue value)
        {
            return $"Value = {value.DoubleValue}";
        }

        private static string GetShortValue(StringSensorValue value)
        {
            return $"Value = {value.StringValue}";
        }

        private static string GetShortValue(IntBarSensorValue value)
        {
            return $"Value: Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}";
        }

        private static string GetShortValue(DoubleBarSensorValue value)
        {
            return $"Value: Min = {value.Min}, Mean = {value.Mean}, Max = {value.Max}, Count = {value.Count}";
        }
        #endregion

        public static ProductDataMessage Convert(Product product)
        {
            ProductDataMessage result = new ProductDataMessage();
            result.Name = product.Name;
            result.Key = product.Key;
            result.DateAdded = product.DateAdded.ToUniversalTime().ToTimestamp();
            return result;
        }

        public static GenerateClientCertificateModel Convert(CertificateRequestMessage requestMessage)
        {
            GenerateClientCertificateModel model = new GenerateClientCertificateModel
            {
                CommonName = requestMessage.CommonName,
                CountryName = requestMessage.CountryName,
                EmailAddress = requestMessage.EmailAddress,
                LocalityName = requestMessage.LocalityName,
                OrganizationName = requestMessage.OrganizationName,
                OrganizationUnitName = requestMessage.OrganizationUnitName,
                StateOrProvinceName = requestMessage.StateOrProvinceName
            };
            return model;
        }

        public static RSAParameters Convert(SensorsService.RSAParameters rsaParameters)
        {
            RSAParameters result = new RSAParameters();
            result.D = rsaParameters.D.ToByteArray();
            result.DP = rsaParameters.DP.ToByteArray();
            result.DQ = rsaParameters.DQ.ToByteArray();
            result.Exponent = rsaParameters.Exponent.ToByteArray();
            result.InverseQ = rsaParameters.InverseQ.ToByteArray();
            result.Modulus = rsaParameters.Modulus.ToByteArray();
            result.P = rsaParameters.P.ToByteArray();
            result.Q = rsaParameters.Q.ToByteArray();
            return result;
        }

        #region Sub-methods

        private static SensorObjectType Convert(SensorDataTypes type)
        {
            switch (type)
            {
                case SensorDataTypes.BoolSensor:
                    return SensorObjectType.ObjectTypeBoolSensor;
                case SensorDataTypes.DoubleSensor:
                    return SensorObjectType.ObjectTypeDoubleSensor;
                case SensorDataTypes.IntSensor:
                    return SensorObjectType.ObjectTypeIntSensor;
                case SensorDataTypes.StringSensor:
                    return SensorObjectType.ObjectTypeStringSensor;
                case SensorDataTypes.BarIntSensor:
                    return SensorObjectType.ObjectTypeBarIntSensor;
                case SensorDataTypes.BarDoubleSensor:
                    return SensorObjectType.ObjectTypeBarDoubleSensor;
            }
            throw new Exception($"Unknown SensorDataType = {type}!");
        }
        private static long GetTimestamp(DateTime dateTime)
        {
            var timeSpan = (dateTime - DateTime.UnixEpoch);
            return (long)timeSpan.TotalSeconds;
        }

        public static void ExtractProductAndSensor(string path, out string server, out string sensor)
        {
            server = string.Empty;
            sensor = string.Empty;
            var splitRes = path.Split("/".ToCharArray());
            server = splitRes[0];
            sensor = splitRes[^1];
        }

        public static string ExtractSensor(string path)
        {
            var splitRes = path.Split("/".ToCharArray());
            return splitRes[^1];
        }
        #endregion

    }
}

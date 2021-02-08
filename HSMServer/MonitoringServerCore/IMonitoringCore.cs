﻿using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HSMSensorDataObjects;
using SensorsService;

namespace HSMServer.MonitoringServerCore
{
    public interface IMonitoringCore
    {
        //public void AddSensorValue(JobResult value);
        Task<bool> AddSensorValueAsync(BoolSensorValue value);
        public void AddSensorValue(BoolSensorValue value);
        public void AddSensorValue(IntSensorValue value);
        public void AddSensorValue(DoubleSensorValue value);
        public void AddSensorValue(StringSensorValue value);
        public void AddSensorValue(IntBarSensorValue value);
        public void AddSensorValue(DoubleBarSensorValue value);
        //public void AddSensorValue(SensorValueBase value);

        //public string AddSensorValue(NewJobResult value);
        public SensorsUpdateMessage GetSensorUpdates(X509Certificate2 clientCertificate);
        public SensorsUpdateMessage GetAllAvailableSensorsUpdates(X509Certificate2 clientCertificate);
        public ProductsListMessage GetProductsList(X509Certificate2 clientCertificate);
        public AddProductResultMessage AddNewProduct(X509Certificate2 clientCertificate, AddProductMessage message);
        public RemoveProductResultMessage RemoveProduct(X509Certificate2 clientCertificate,
            RemoveProductMessage message);
        public SensorHistoryListMessage GetSensorHistory(X509Certificate2 clientCertificate, GetSensorHistoryMessage getHistoryMessage);

        public SignedCertificateMessage SignClientCertificate(X509Certificate2 clientCertificate,
            CertificateSignRequestMessage request);
    }
}

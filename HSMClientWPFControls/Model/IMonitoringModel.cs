﻿using System.Collections.ObjectModel;
using HSMClientWPFControls.ConnectorInterface;
using HSMClientWPFControls.Objects;
using HSMClientWPFControls.ViewModel;

namespace HSMClientWPFControls.Model
{
    public interface IMonitoringModel
    {
        public bool IsClientCertificateDefault { get; }
        public IProductsConnector ProductsConnector { get; }
        public ISensorHistoryConnector SensorHistoryConnector { get; }
        ObservableCollection<MonitoringNodeBase> Nodes { get; set; }
        ObservableCollection<ProductViewModel> Products { get; set; }
        void Dispose();
        void ShowProducts();
        void UpdateProducts();
        void RemoveProduct(ProductInfo product);
        ProductInfo AddProduct(string name);
    }
}

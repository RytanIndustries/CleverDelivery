using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace CleverDelivery.ViewModels
{
    public class RoutingViewModel
    {
        private Map _map;
        private BasemapType basemapType = BasemapType.StreetsNightVector;
        private double latitude = 34.05293;
        private double longitude = -118.24368;
        private int levelOfDetail = 11;
        private string trafficLayerURL = "https://traffic.arcgis.com/arcgis/rest/services/World/Traffic/MapServer";
        private string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private string ClientId = "8NGRcFKX4pIqjJc4";
        private string ClientSecret = "b7a398bcb2984f03a537c21e92f220a4";
        private string RedirectURI = "my-app://auth";


        public RoutingViewModel()
        {
            CreateNewMap();
            AddTrafficLayer();

            
        }
        /// <summary>
        /// Gets or sets the map
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set { _map = value; }
        }
        private void CreateNewMap()
        {
            Map = new Map(basemapType, latitude, longitude, levelOfDetail);
        }
        private void AddTrafficLayer()
        {
            // SetOAuthInfo();
            ArcGISMapImageLayer traffic = new ArcGISMapImageLayer(new Uri(trafficLayerURL));
            Map.OperationalLayers.Add(traffic);
        }


    }
}

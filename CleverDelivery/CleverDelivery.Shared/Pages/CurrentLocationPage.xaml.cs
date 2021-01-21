using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Core;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using CleverDelivery.ViewModels;
using Esri.ArcGISRuntime.Mapping;
using Map = Esri.ArcGISRuntime.Mapping.Map;
#if XAMARIN_ANDROID
using Esri.ArcGISRuntime.Xamarin.Forms.Platform.Android;

#endif

namespace CleverDelivery.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentLocationPage : CorePage
    {
        
        public CurrentLocationPage()
        {
            InitializeComponent();
            Initialize();
            var vm = CoreDependencyService.GetViewModel<SearchViewModel>();
            vm.MapView = MyMapView;

        }
        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            MyMapView.Map = myMap;
        }
        private void OnStopClicked(object sender, EventArgs e)
        {
            MyMapView.LocationDisplay.IsEnabled = false;
        }

        private async void OnStartClicked(object sender, EventArgs e)
        {
            
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}
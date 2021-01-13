using CleverDelivery.ViewModels;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Core;
using Xamarin.Forms.Xaml;

namespace CleverDelivery.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddressSearchPage : CorePage
    {
        

        // The LocatorTask provides geocoding services
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public AddressSearchPage()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            var vm = CoreDependencyService.GetViewModel<SearchViewModel>();
            vm.MapView = MyMapView;
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImageryWithLabelsVector());

            // Assign the map to the MapView
            MyMapView.Map = myMap;
            
            try
            {
                // Initialize the LocatorTask with the provided service Uri
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);
                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;

#if XAMARIN_ANDROID
                // See implementation in MainActivity.cs in the Android platform project.
                MainActivity.Instance.AskForLocationPermission(MyMapView);
#else
                await MyMapView.LocationDisplay.DataSource.StartAsync();
                MyMapView.LocationDisplay.IsEnabled = true;
#endif

                // Enable the UI controls now that the LocatorTask is ready
                MySuggestButton.IsEnabled = true;
                MySearchBar.IsEnabled = true;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }        

        /// <summary>
        /// Handle tap event on the map; displays callouts showing the address for a tapped search result
        /// </summary>
        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            try
            {
                // Search for the graphics underneath the user's tap
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

                // Return gracefully if there was no result
                if (results.Count < 1 || results.First().Graphics.Count < 1) { return; }

                // Reverse geocode to get addresses
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location);

                // Get the first result
                GeocodeResult address = addresses.First();
                // Use the city and region for the Callout Title
                string calloutTitle = address.Attributes["City"] + ", " + address.Attributes["Region"];
                // Use the metro area for the Callout Detail
                string calloutDetail = address.Attributes["MetroArea"].ToString();

                // Define the callout
                CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

                // Show the callout on the map at the tapped location
                MyMapView.ShowCalloutAt(e.Location, calloutBody);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }
}
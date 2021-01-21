using CleverDelivery.Pages;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Core;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace CleverDelivery.ViewModels
{
    public class SearchViewModel : CoreViewModel
    {
        #region Commands

        public ICommand AddressSearchCommand { get; set; }
        public ICommand StartGeolocation { get; set; }

        #endregion Commands

        #region Properties

        // Store a reference to the map view (to set the view extent and add graphics)
        public MapView MapView { get; set; }
        public string SearchTerm { get; set; }
        // The LocatorTask provides geocoding services
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");
        public ObservableCollection<MapPoint> RoutePoints { get; set; }
        private Map _map = new Map(Basemap.CreateStreets());

        public Map Map
        {
            get { return _map; }
            set { _map = value; }
        }
        // String array to store the different device location options.
        private string[] _navigationTypes =
        {
            "On",
            "Re-Center",
            "Navigation",
            "Compass"
        };
        #endregion Properties

        public SearchViewModel()
        {
            
            _map.InitialViewpoint = new Viewpoint(34.05293, -118.24368, 6000);
            AddressSearchCommand = new CoreCommand(async (obj)=> 
            {
                // Initialize the LocatorTask with the provided service Uri
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);
                await UpdateSearch();
            });
            StartGeolocation = new CoreCommand(async (obj) =>
            {
                // Initialize the LocatorTask with the provided service Uri
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);
                await UpdateCurrentLocation();
            });
        }
        public override void OnViewMessageReceived(string key, object obj)
        {
            switch (key)
            {
                case "ChangeMap":
                    {
                        break;
                    }
            }
        }
        public async override void OnInit()
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var cts = new CancellationTokenSource();
            var location = await Geolocation.GetLocationAsync(request, cts.Token);
            _map.InitialViewpoint = new Viewpoint(location.Latitude, location.Longitude, 6000);
            MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
            await MapView.LocationDisplay.DataSource.StartAsync();
            MapView.LocationDisplay.IsEnabled = true;
        }

        public async Task UpdateSearch()
        {

            // Get the text in the search bar
            string enteredText = SearchTerm;

            // Clear existing marker
            MapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready
            if (String.IsNullOrWhiteSpace(enteredText) || _geocoder == null) { return; }

            try
            {
                // Get suggestions based on the input text
                IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(enteredText);

                // Stop gracefully if there are no suggestions
                if (suggestions.Count < 1) { return; }

                // Get the full address for the first suggestion
                SuggestResult firstSuggestion = suggestions.First();
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(firstSuggestion.Label);

                // Stop gracefully if the geocoder does not return a result
                if (addresses.Count < 1) { return; }

                // Place a marker on the map - 1. Create the overlay
                GraphicsOverlay resultOverlay = new GraphicsOverlay();
                // 2. Get the Graphic to display
                Graphic point = await GraphicForPoint(addresses.First().DisplayLocation);
                // 3. Add the Graphic to the GraphicsOverlay
                resultOverlay.Graphics.Add(point);
                // 4. Add the GraphicsOverlay to the MapView
                MapView.GraphicsOverlays.Add(resultOverlay);

                // Update the map extent to show the marker
                MapView.SetViewpoint(new Viewpoint(addresses.First().Extent));
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
#if WINDOWS_UWP
            // Get current assembly that contains the image
            Assembly currentAssembly = GetType().GetTypeInfo().Assembly;
#else
            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
#endif

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            var resources = currentAssembly.GetManifestResourceNames();
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "CleverDelivery.Images.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        private async Task UpdateCurrentLocation()
        {
            MapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
            await MapView.LocationDisplay.DataSource.StartAsync();
            MapView.LocationDisplay.IsEnabled = true;
            

            try
            {
                // Permission request only needed on Android.
#if XAMARIN_ANDROID
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert("Permission Requested", "We need to use your location service in order to show your current location on a map", "OKAY");
                    var wasGranted = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (wasGranted != PermissionStatus.Granted)
                        Application.Current.MainPage = new NavigationPage(new StartPage());
                }

                await MapView.LocationDisplay.DataSource.StartAsync();
                MapView.LocationDisplay.IsEnabled = true;
                MapView.LocationDisplay.InitialZoomScale = 60000;
#else
                await MapView.LocationDisplay.DataSource.StartAsync();
                MapView.LocationDisplay.IsEnabled = true;
#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
            }
        }
    }
}
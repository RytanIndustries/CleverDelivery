using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Core;
using Xamarin.Forms.Xaml;

namespace CleverDelivery.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OptimizedRoutingPage : CorePage
    {
        private string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private string ClientId = "8NGRcFKX4pIqjJc4";
        private string ClientSecret = "b7a398bcb2984f03a537c21e92f220a4";
        private string RedirectURI = "my-app://auth";
        // List of stops on the route ('from' and 'to')
        private List<Stop> _routeStops;

        // Graphics overlay to display stops and the route result
        private GraphicsOverlay _routeGraphicsOverlay;

        // URI for the San Diego route service
        private Uri _sanDiegoRouteServiceUri = new Uri("https://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World");

        // URIs for picture marker images
        private Uri _checkedFlagIconUri = new Uri("https://static.arcgis.com/images/Symbols/Transportation/CheckeredFlag.png");

        private Uri _carIconUri = new Uri("https://static.arcgis.com/images/Symbols/Transportation/CarRedFront.png");
        public OptimizedRoutingPage()
        {
            InitializeComponent();
            
            // Create the map, graphics overlay, and the 'from' and 'to' locations for the route
            Initialize();
        }
        private void Initialize()
        {

            // Define the route stop locations (points)
            MapPoint fromPoint = new MapPoint(-112.068962, 33.638390, SpatialReferences.Wgs84);
            MapPoint toPoint = new MapPoint(-112.1028099, 33.7334937, SpatialReferences.Wgs84);

            // Create Stop objects with the points and add them to a list of stops
            Stop stop1 = new Stop(new MapPoint(-112.068962, 33.638390,  SpatialReferences.Wgs84));
            Stop stop2 = new Stop(new MapPoint(-111.994930, 33.618900, SpatialReferences.Wgs84));
            Stop stop3 = new Stop(new MapPoint(-112.0021089, 33.6858299, SpatialReferences.Wgs84));
            Stop stop4 = new Stop(new MapPoint(-111.9734644, 33.6348065, SpatialReferences.Wgs84));
            Stop stop5 = new Stop(new MapPoint(-112.1028099, 33.7334937, SpatialReferences.Wgs84));
            _routeStops = new List<Stop> { stop1, stop2, stop3, stop4, stop5 };

            //// Create Stop objects with the points and add them to a list of stops
            //Stop stop1 = new Stop(fromPoint);
            //Stop stop2 = new Stop(toPoint);
            //_routeStops = new List<Stop> { stop1, stop2 };

            // Picture marker symbols: from = car, to = checkered flag
            PictureMarkerSymbol carSymbol = new PictureMarkerSymbol(_carIconUri);
            PictureMarkerSymbol flagSymbol = new PictureMarkerSymbol(_checkedFlagIconUri);

            // Add a slight offset (pixels) to the picture symbols.
            carSymbol.OffsetX = -carSymbol.Width / 2;
            carSymbol.OffsetY = -carSymbol.Height / 2;
            flagSymbol.OffsetX = -flagSymbol.Width / 2;
            flagSymbol.OffsetY = -flagSymbol.Height / 2;

            // Set the height and width.
            flagSymbol.Height = 60;
            flagSymbol.Width = 60;
            carSymbol.Height = 60;
            carSymbol.Width = 60;

            // Create graphics for the stops
            Graphic fromGraphic = new Graphic(fromPoint, carSymbol) { ZIndex = 1 };
            Graphic toGraphic = new Graphic(toPoint, flagSymbol) { ZIndex = 1 };

            // Create the graphics overlay and add the stop graphics
            _routeGraphicsOverlay = new GraphicsOverlay();
            _routeGraphicsOverlay.Graphics.Add(fromGraphic);
            _routeGraphicsOverlay.Graphics.Add(toGraphic);

            // Get an Envelope that covers the area of the stops (and a little more)
            Envelope routeStopsExtent = new Envelope(fromPoint, toPoint);
            EnvelopeBuilder envBuilder = new EnvelopeBuilder(routeStopsExtent);
            envBuilder.Expand(1.5);

            // Create a new viewpoint apply it to the map view when the spatial reference changes
            Viewpoint sanDiegoViewpoint = new Viewpoint(envBuilder.ToGeometry());
            MyMapView.SpatialReferenceChanged += (s, e) => MyMapView.SetViewpoint(sanDiegoViewpoint);

            // Add a new Map and the graphics overlay to the map view
            MyMapView.Map = new Map(Basemap.CreateImageryWithLabelsVector());
            MyMapView.GraphicsOverlays.Add(_routeGraphicsOverlay);
        }

        private async void SolveRouteClick(object sender, EventArgs e)
        {
            try
            {

                // Register a portal that uses OAuth authentication with the AuthenticationManager 
                Esri.ArcGISRuntime.Security.ServerInfo serverInfo = new ServerInfo(new Uri("https://www.arcgis.com/sharing/rest"));
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthClientCredentials;
                serverInfo.OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret,
                    RedirectUri = new Uri(RedirectURI)
                };

                 
                AuthenticationManager.Current.RegisterServer(serverInfo);
                // Create a new route task using the San Diego route service URI
                RouteTask solveRouteTask = await RouteTask.CreateAsync(_sanDiegoRouteServiceUri);

                // Get the default parameters from the route task (defined with the service)
                RouteParameters routeParams = await solveRouteTask.CreateDefaultParametersAsync();
                routeParams.PreserveLastStop = false;
                routeParams.DirectionsStyle = DirectionsStyle.Navigation;

                //TravelMode travel = new TravelMode();
                var travelModes = solveRouteTask.RouteTaskInfo.TravelModes.ToList();
                // Make some changes to the default parameters
                routeParams.ReturnStops = true;
                routeParams.ReturnDirections = true;
                routeParams.FindBestSequence = true;
                routeParams.DirectionsDistanceUnits = Esri.ArcGISRuntime.UnitSystem.Imperial;

                // Set the list of route stops that were defined at startup
                routeParams.SetStops(_routeStops);

                // Solve for the best route between the stops and store the result
                RouteResult solveRouteResult = await solveRouteTask.SolveRouteAsync(routeParams);

                // Get the first (should be only) route from the result
                var min = solveRouteResult.Routes.Min(x => x.TotalLength);
                Route firstRoute = solveRouteResult.Routes.FirstOrDefault(x=>x.TotalLength == min);

                // Get the route geometry (polyline)
                Polyline routePolyline = firstRoute.RouteGeometry;

                // Create a thick purple line symbol for the route
                SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Purple, 8.0);

                // Create a new graphic for the route geometry and add it to the graphics overlay
                Graphic routeGraphic = new Graphic(routePolyline, routeSymbol) { ZIndex = 0 };
                _routeGraphicsOverlay.Graphics.Add(routeGraphic);

                // Get a list of directions for the route and display it in the list box
                DirectionsListBox.ItemsSource = firstRoute.DirectionManeuvers.Select(direction => direction.DirectionText);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void ResetClick(object sender, EventArgs e)
        {
            // Clear the list of directions
            DirectionsListBox.ItemsSource = null;

            // Remove the route graphic from the graphics overlay (only line graphic in the collection)
            int graphicsCount = _routeGraphicsOverlay.Graphics.Count;
            for (int i = graphicsCount; i > 0; i--)
            {
                // Get this graphic and see if it has line geometry
                Graphic g = _routeGraphicsOverlay.Graphics[i - 1];
                if (g.Geometry.GeometryType == GeometryType.Polyline)
                {
                    // Remove the graphic from the overlay
                    _routeGraphicsOverlay.Graphics.Remove(g);
                }
            }
        }

        private void ShowHideDirectionsList(object sender, EventArgs e)
        {
            // Show the directions frame if it's hidden; hide if it's shown
            DirectionsFrame.IsVisible = !DirectionsFrame.IsVisible;
        }
    }
}
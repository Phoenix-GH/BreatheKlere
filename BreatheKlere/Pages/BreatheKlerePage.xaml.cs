/* Main Page */
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using BreatheKlere.REST;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BreatheKlere
{
    public partial class BreatheKlerePage : ContentPage
    {
        // mode variables
        public byte mapMode = 0;
        bool isFirstLaunch;
        public byte isHomeSet = 0, isDestinationSet = 0;
        string[] modes = { "bicycling", "walking" };
        string[] mqModes = { "bicycle", "pedestrian" };
        float maxPollution = 0, maxTime = 0, cleanestTime = 0, fastestPollution = 0, fastestDistance = 0, cleanestDistance = 0;
        int mode = 1;

        RESTService rest;

        //location variables
        public string origin, destination, currentPos;
        public Position originPos, destinationPos;

        // Point array 

        Pin startPin, endPin, hotspotPin;
        Position hotspot;
        float peak = 0;
        Xamarin.Forms.GoogleMaps.Polyline fastest, cleanest;
        string DID;
        public BreatheKlerePage()
        {
            InitializeComponent();
            // MapTypes
            var mapTypeValues = new List<MapType>();
            rest = new RESTService();
            foreach (var mapType in Enum.GetValues(typeof(MapType)))
            {
                mapTypeValues.Add((MapType)mapType);
            }
            if(App.Current.Properties.ContainsKey("DID"))
                DID = App.Current.Properties["DID"].ToString();
            isFirstLaunch = true;
            map.MapType = mapTypeValues[0];
            map.MyLocationEnabled = true;
            map.IsTrafficEnabled = true;
            map.IsIndoorEnabled = false;
            map.UiSettings.CompassEnabled = true;
            map.UiSettings.RotateGesturesEnabled = true;
            map.UiSettings.MyLocationButtonEnabled = true;
            map.UiSettings.IndoorLevelPickerEnabled = false;
            map.UiSettings.ScrollGesturesEnabled = true;
            map.UiSettings.TiltGesturesEnabled = false;
            map.UiSettings.ZoomControlsEnabled = true;
            map.UiSettings.ZoomGesturesEnabled = true;

         
            var entryGesture = new TapGestureRecognizer();
            entryGesture.Tapped += Home_Focused;
            entryAddress.GestureRecognizers.Add(entryGesture);

            var destinationGesture = new TapGestureRecognizer();
            destinationGesture.Tapped += Destination_Focused;
            destinationAddress.GestureRecognizers.Add(destinationGesture);

            startPin = new Pin
            {
                Type = PinType.SavedPin,
                Label = "Start Point",
            };
            endPin = new Pin
            {
                Type = PinType.Generic,
                Label = "End Point",
            };
            // Map Clicked

            map.MapClicked += async (sender, e) =>
            {
                if (mapMode == 2)
                {
                    destinationPos = e.Point;
                    endPin.Position = e.Point;

                    GeoResult result = await rest.GetGeoResult(e.Point.Latitude.ToString() + ',' + e.Point.Longitude.ToString());
                    if (result != null)
                    {
                        destinationAddress.Text = result.results[0].formatted_address;
                        destination = result.results[0].formatted_address;
                        endPin.Address = result.results[0].formatted_address;
                    }
                    if (map.Pins.Contains(endPin))
                        map.Pins.Remove(endPin);
                    map.Pins.Add(endPin);
                    isDestinationSet = 2;
                    await CalculateRoute();
                }
                else if (mapMode == 1)
                {
                    originPos = e.Point;
                    startPin.Position = e.Point;

                    GeoResult result = await rest.GetGeoResult(e.Point.Latitude.ToString() + ',' + e.Point.Longitude.ToString());
                    if (result != null)
                    {
                        origin = result.results[0].formatted_address;
                        entryAddress.Text = result.results[0].formatted_address;
                        startPin.Address = result.results[0].formatted_address;
                    }
                    if (map.Pins.Contains(startPin))
                        map.Pins.Remove(startPin);
                    map.Pins.Add(startPin);
                    isHomeSet = 2;
                }

                mapMode = 0;

            };

        }

        void ReverseRoute() 
        {
            var temp = origin;
            origin = destination;
            destination = temp;

            var tempPos = originPos;
            originPos = destinationPos;
            destinationPos = tempPos;

            var isSet = isHomeSet;
            isHomeSet = isDestinationSet;
            isDestinationSet = isSet;

            entryAddress.Text = origin;
            destinationAddress.Text = destination;
        }

		async protected override void OnAppearing()
		{
            base.OnAppearing();
            NavigationPage.SetHasNavigationBar(this, false);
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await DisplayAlert("Need location", "Gunna need that location", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Location))
                        status = results[Permission.Location];
                }
           
                if (status == PermissionStatus.Granted)
                {
                    if (Utils.IsLocationAvailable() && isFirstLaunch)
                    {
                        originPos = await Utils.GetPosition();
                        currentPos = originPos.Latitude + "," + originPos.Longitude;
                        map.MoveToRegion(MapSpan.FromCenterAndRadius(originPos, Distance.FromMeters(5000)));
                        GeoResult result = await rest.GetGeoResult(currentPos);
                        if (result != null)
                        {
                            origin = result.results[0].formatted_address;
                        }
                        else
                        {
                            origin = currentPos;
                        }
                       
                        isHomeSet = 2;
                    }
                }
                else
                {
                    await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }

                if (isFirstLaunch)
                {
                    isFirstLaunch = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

            //Setting up the locations
            if(isHomeSet > 0)
            {
                if(isHomeSet == 1)
                {
                    var result = await rest.GetGeoResult(origin);
                    if (result != null)
                        originPos = new Position(result.results[0].geometry.location.lat, result.results[0].geometry.location.lng);
                }

                entryAddress.Text = origin;
                startPin.Address = origin;
                if (map.Pins.Contains(startPin))
                    map.Pins.Remove(startPin);
                
                startPin.Position = originPos;
                map.Pins.Add(startPin);
            }

            if (isDestinationSet > 0)
            {
                if (isDestinationSet == 1)
                {
                    var result = await rest.GetGeoResult(destination);
                    if (result != null)
                        destinationPos = new Position(result.results[0].geometry.location.lat, result.results[0].geometry.location.lng);
                }
                destinationAddress.Text = destination;
                endPin.Address = destination;
                if (map.Pins.Contains(endPin))
                    map.Pins.Remove(endPin);
                endPin.Position = destinationPos;
                map.Pins.Add(endPin);

            }
            if(isHomeSet > 0 && isDestinationSet > 0)
                await CalculateRoute();
          
		}

        private List<Position> DecodePolyline(string encodedPoints)
        {
            if (string.IsNullOrWhiteSpace(encodedPoints))
                return null;

            int index = 0;
            var polylineChars = encodedPoints.ToCharArray();
            var poly = new List<Position>();
            int currentLat = 0;
            int currentLng = 0;
            int next5Bits;

            while (index < polylineChars.Length)
            {
                int sum = 0;
                int shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                    break;

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);  
                sum = 0;
                shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                {
                    break;
                }

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);
                var mLatLng = new Position(Convert.ToDouble(currentLat) / 100000.0, Convert.ToDouble(currentLng) / 100000.0);
                poly.Add(mLatLng);
            }

            return poly;
        }

        void Home_Focused(object sender, EventArgs e)
        {
            isHomeSet = 0;
            entryAddress.Unfocus();
            Navigation.PushModalAsync(new LocationSelectionPage(this, true));
        }

        void Destination_Focused(object sender, EventArgs e)
        {
            isDestinationSet = 0;
            destinationAddress.Unfocus();
            Navigation.PushModalAsync(new LocationSelectionPage(this, false));
        }

        async void Walking_Clicked(object sender, System.EventArgs e)
        {
            btnWalking.IsEnabled = false;
            btnBicycling.IsEnabled = false;
            RouteReset();
            clearStyles();
            mode = 1;
            await CalculateRoute();
            btnWalking.BackgroundColor = Color.White;
            btnWalking.IsEnabled = true;
            btnBicycling.IsEnabled = true;
        }

        async void Bicycling_Clicked(object sender, System.EventArgs e)
        {
            btnWalking.IsEnabled = false;
            btnBicycling.IsEnabled = false;
            RouteReset();
            clearStyles();
            mode = 0;
            await CalculateRoute();
            btnBicycling.BackgroundColor = Color.White;
            btnWalking.IsEnabled = true;
            btnBicycling.IsEnabled = true;
        }

        void Reverse_Clicked(object sender, System.EventArgs e)
        {
            ReverseRoute();
        }

        void clearStyles()
        {
            btnWalking.BackgroundColor = Color.Gray;
            btnBicycling.BackgroundColor = Color.Gray;
        }

        async Task<bool> CalculateRoute() 
        {
            buttonGrid.IsVisible = false;
            map.Polylines.Clear();
            RouteReset();
            var line1 = new Xamarin.Forms.GoogleMaps.Polyline();
            var line2 = new Xamarin.Forms.GoogleMaps.Polyline();

            fastest = new Xamarin.Forms.GoogleMaps.Polyline();
            cleanest = new Xamarin.Forms.GoogleMaps.Polyline();
            fastest.StrokeColor = Color.Blue;
            fastest.StrokeWidth = 7;
            cleanest.StrokeColor = Color.FromHex("00e36f");
            cleanest.StrokeWidth = 4;

            line1.StrokeColor = Color.Blue;
            line1.StrokeWidth = 7;
            line2.StrokeColor = Color.FromHex("00e36f");
            line2.StrokeWidth = 4;

            string originParam = originPos.Latitude.ToString() + ',' + originPos.Longitude.ToString();
            string destinationParam = destinationPos.Latitude.ToString() + ',' + destinationPos.Longitude.ToString();

            line1.Positions.Clear();
            line2.Positions.Clear();
            fastest.Positions.Clear();
            cleanest.Positions.Clear();

            maxTime = 0;
            fastestDistance = 0;
            cleanestDistance = 0;
            fastestPollution = 0;
            cleanestTime = 0;
            maxPollution = 0;

            if (!string.IsNullOrEmpty(originParam) && !string.IsNullOrEmpty(destinationParam) && isHomeSet>0 && isDestinationSet>0)
            {
                Bounds bounds = new Bounds(originPos, destinationPos);
                map.MoveToRegion(MapSpan.FromBounds(bounds));
                blueDistanceLabel.Text = "";
                magentaDistanceLabel.Text = "";
               
                await GetHeatMap(bounds);

                var mqResult = await rest.GetMQAlternativeDirection(originParam, destinationParam, mqModes[mode]);

                List<Position> pollutionPoints = new List<Position>();
 
                bool isRouteChosen = false;
                if (mqResult != null)
                {
                    if (mqResult.route != null)
                    {
                        if (mqResult.route.shape != null)
                        {
                            fastest.Positions.Clear();
                            cleanest.Positions.Clear();
                            if (!string.IsNullOrEmpty(mqResult.route.shape.shapePoints))
                            {
                                var points = DecodePolyline(mqResult.route.shape.shapePoints);
                                foreach (var point in points)
                                {
                                    fastest.Positions.Add(point);
                                    cleanest.Positions.Add(point);
                                    pollutionPoints.Add(point);
                                }
                                maxPollution = await CalculatePollution(pollutionPoints, true);
                                fastestPollution = maxPollution;
                                maxTime = mqResult.route.time;
                                cleanestTime = mqResult.route.time;
                                fastestDistance = mqResult.route.distance;
                                cleanestDistance = mqResult.route.distance;
                            }
                        }

                    }
                    MQDirection fastestRoute = new MQDirection();
                    MQDirection cleanestRoute = new MQDirection();
                   
                    if (mqResult.route.alternateRoutes != null)
                    {
                        if(mqResult.route.alternateRoutes.Count > 0)
                        {
                            bool duplicated = false;
                            map.Polylines.Clear();
                            fastestRoute = mqResult.route.alternateRoutes[0];
                            cleanestRoute = mqResult.route.alternateRoutes[0];
                            foreach (var item in mqResult.route.alternateRoutes)
                            {
                                
                                line1.Positions.Clear();
                                line2.Positions.Clear();
                                pollutionPoints.Clear();
                                if (item.route.shape != null)
                                {

                                    if (!string.IsNullOrEmpty(item.route.shape.shapePoints))
                                    {

                                        var points = DecodePolyline(item.route.shape.shapePoints);
                                        foreach (var point in points)
                                        {
                                            line1.Positions.Add(point);
                                            line2.Positions.Add(point);
                                            pollutionPoints.Add(point);
                                            if (point.Latitude.Equals(hotspot.Latitude) && point.Longitude.Equals(hotspot.Longitude))
                                            {
                                                duplicated = true;
                                                break;
                                            }
                                        }
                                        if (!duplicated)
                                        {
                                            float pollutionValue = await CalculatePollution(pollutionPoints, true);
                                            if (item.route.time < maxTime)
                                            {
                                                fastest = line1;
                                                fastestRoute = item;
                                                maxTime = item.route.time;
                                                fastestPollution = pollutionValue;
                                                fastestDistance = item.route.distance;
                                                isRouteChosen = true;
                                            } 
                                            else if (pollutionValue < maxPollution)
                                            {
                                                cleanest = line2;
                                                cleanestRoute = item;
                                                maxPollution = pollutionValue;
                                                cleanestTime = item.route.time;
                                                cleanestDistance = item.route.distance;
                                                isRouteChosen = true;
                                            }
                                        }
                                    }
                                }
                            }
                            if (isRouteChosen && fastestPollution > maxPollution)
                            {
                                map.Polylines.Clear();

                                if (fastest.Positions.Count >= 2)
                                {
                                    map.Polylines.Add(fastest);
                                }
                                if (cleanest.Positions.Count >= 2)
                                {
                                    map.Polylines.Add(cleanest);
                                }

                                blueDistanceLabel.Text = $"{fastestDistance.ToString("F1")} miles {timeToMin(maxTime)} Pollution: {(int)fastestPollution}";
                                magentaDistanceLabel.Text = $"{cleanestDistance.ToString("F1")} miles {timeToMin(cleanestTime)} Pollution: {(int)maxPollution}";
                                buttonGrid.IsVisible = true;
                                drawHotspot();
                                return true;
                            }
                        }
         
                    }

                    buttonGrid.IsVisible = false;
                    map.Polylines.Clear();
                    Device.StartTimer(TimeSpan.FromMilliseconds(3000), () =>
                    {
                        if (fastest.Positions.Count >= 2)
                        {
                            fastest.StrokeColor = Color.FromHex("00e36f");
                            map.Polylines.Add(fastest);
                        }

                        magentaDistanceLabel.Text = $"{mqResult.route.distance.ToString("F1")} miles {timeToMin(mqResult.route.time)} Pollution: {(int)fastestPollution}";
                        blueDistanceLabel.BackgroundColor = Color.Gray;
                        blueDistanceLabel.Text = "No faster route available.";
                        drawHotspot();
                        return false;
                    });
                   
                }
            }
            else
            {
                await DisplayAlert("Warning", "Please fill all the fields", "OK");
            }
            return false;
        }

        
        async Task<float> CalculatePollution(List<Position> pollutionPoints, bool main=false) 
        {

            float overall = 0, max = 0;
            if(pollutionPoints.Count > 0) 
            {
                List<List<string>> list = new List<List<string>>();
                for (int i = 0; i < pollutionPoints.Count; i++)
                {
                    List<string> point = new List<string>();
                    point.Add(pollutionPoints[i].Latitude.ToString("F6"));
                    point.Add(pollutionPoints[i].Longitude.ToString("F6"));
                    list.Add(point);
                }
                PollutionRequest request = new PollutionRequest()
                {
                    RAD = 50,
                    PAIRS = list
                };
                var result = await rest.GetPollution(JsonConvert.SerializeObject(request));
                if(result!=null)
                {
                    int maxIndex = 0;
                    if (result.val.Count >= 5)
                    {
                        for (int index = 2; index < result.val.Count - 2; index++)
                        {
                            float subTotal = 0;
                            for (int j = index - 2; j < index + 2; j++)
                            {
                                subTotal += (float)Convert.ToDouble(result.val[j]);
                            }
                            if (subTotal > max)
                            {
                                max = subTotal;
                                maxIndex = index;
                                peak = max;
                            }
                  
                            overall += (float)Convert.ToDouble(result.val[index]);
                        }
                        overall /= result.val.Count;
                        double lat, lng;
                        lat = Convert.ToDouble(result.lat[maxIndex]);
                        lng = Convert.ToDouble(result.lon[maxIndex]);
                        hotspot = new Position(lat, lng);

                    }

                }
            }
           
            return overall;
        }

        async Task<bool> GetHeatMap(Bounds bounds)
        {
            // set the size of the pixel in degrees lat / lon
            map.Polygons.Clear();
            map.Polylines.Clear();
            // build the request for polution info
            var request = new PollutionRequest();
            request.RAD = 100;
            request.PAIRS = new List<List<string>>();

            var radius = Math.Max(bounds.HeightDegrees / 2, bounds.WidthDegrees / 2);
            var unit = Math.Max(.0015, radius / 100);
            var halfU = unit / 2;

            var left = Convert.ToDouble((bounds.Center.Longitude + radius * 2).ToString("F6"));
            var right = Convert.ToDouble((bounds.Center.Longitude - radius * 2).ToString("F6"));
            var top = Convert.ToDouble((bounds.Center.Latitude + radius).ToString("F6"));
            var bottom = Convert.ToDouble((bounds.Center.Latitude - radius).ToString("F6"));


            for (var Y = bottom; Y <= top; Y += unit)
            {
                for (var X = left; X >= right; X -= unit)
                {
                    List<string> point = new List<string>();
                   
                    point.Add(Y.ToString());
                    point.Add(X.ToString());

                    (request.PAIRS).Add(point);
                    if(request.PAIRS.Count >= 400 || (Y+unit > top && X-unit < right))
                    {
                        var result = await rest.GetPollution(JsonConvert.SerializeObject(request));
                        if (result != null)
                        {
                            // get the results
                            var x = 0;
                            double lvl = 0;
                            // loop through the results
                            for (x = 0; x < result.lon.Count; x++)
                            {
                                string level = (result.val)[x];
                                lvl = Convert.ToDouble(level) * 1;
                                Color fc = Color.Black;

                                if (lvl <= 50)
                                {
                                    fc = Color.Green;
                                }
                                else
                                {
                                    if (lvl <= 100)
                                    {
                                        fc = Color.Yellow;
                                    }
                                    else
                                    {
                                        fc = Color.Red;
                                    }
                                }

                                var north = Convert.ToDouble(result.lat[x]) * 1 + halfU * 1;
                                var south = Convert.ToDouble(result.lat[x]) * 1 - halfU * 1;

                                var east = Convert.ToDouble(result.lon[x]) * 1 + halfU * 1;
                                var west = Convert.ToDouble(result.lon[x]) * 1 - halfU * 1;
                                var tileBounds = new Bounds(new Position(south, west), new Position(north, east));
                                var rectangle = new Xamarin.Forms.GoogleMaps.Polygon();
 
                                rectangle.StrokeWidth = 0;
                                rectangle.FillColor = Color.FromRgba(255, 0, 0, (lvl / 30.0) - 0.25);

                                rectangle.Positions.Add(tileBounds.NorthEast);
                                rectangle.Positions.Add(tileBounds.NorthWest);
                                rectangle.Positions.Add(tileBounds.SouthWest);
                                rectangle.Positions.Add(tileBounds.SouthEast);

                                map.Polygons.Add(rectangle);

                            }

                        }
                        request.PAIRS.Clear();
                    }
                }
            }
            return true;
        }

        void drawHotspot()
        {
            map.Pins.Remove(hotspotPin);
            hotspotPin = new Pin
            {
                Type = PinType.SavedPin,
                Label = "Hotspot: " + peak.ToString(),
                Position = hotspot,
            };
            map.Pins.Add(hotspotPin);
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            if(mapMode == 2)
                Navigation.PushModalAsync(new LocationSelectionPage(this, false));
            else
                Navigation.PushModalAsync(new LocationSelectionPage(this, true));
        }

        void Handle_Fastest(object sender, System.EventArgs e)
        {
            RouteReset();
            magentaDistanceLabel.BackgroundColor = Color.Gray;
            btnCleanest.BackgroundColor = Color.Gray;
            if(fastest.Positions.Count >= 2)
                map.Polylines.Add(fastest);
        }

        void Handle_Cleanest(object sender, System.EventArgs e)
        {
            RouteReset();
            blueDistanceLabel.BackgroundColor = Color.Gray;
            btnFastest.BackgroundColor = Color.Gray;
            if(cleanest.Positions.Count >=2 )
                map.Polylines.Add(cleanest);
        }

        void RouteReset()
        {
            map.Polylines.Clear();
            blueDistanceLabel.BackgroundColor = Color.FromHex("2196F3");
            btnFastest.BackgroundColor = Color.FromHex("2196F3");
            magentaDistanceLabel.BackgroundColor = Color.FromHex("00e36f");
            btnCleanest.BackgroundColor = Color.FromHex("00e36f");
        }

        string timeToMin(float time)
        {
            int min = (int)time / 60 % 60;
            int hour = (int)time / 3600;

            return $"{ hour}:{ min} mins";
        }
    }
}

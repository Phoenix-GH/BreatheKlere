using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using BreatheKlere.REST;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BreatheKlere
{
    public partial class BreatheKlerePage : ContentPage
    {
        // mode variables
        public byte mapMode = 0;
        bool isFirstLaunch;
        public byte isHomeSet = 0, isDestinationSet = 0;
        string[] modes = { "driving", "walking", "bicycling" };
        int mode = 0;

        RESTService rest;

        //location variables
        public string origin, destination, currentPos;
        public Position originPos, destinationPos;
        Pin startPin, endPin;

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
                        setDestinationStatus(result.results[0].formatted_address, "Destination Address");
                        destination = result.results[0].formatted_address;
                        endPin.Address = result.results[0].formatted_address;
                    }
                    if (map.Pins.Contains(endPin))
                        map.Pins.Remove(endPin);
                    map.Pins.Add(endPin);
                    isDestinationSet = 2;
                }
                else if (mapMode == 1)
                {
                    originPos = e.Point;
                    startPin.Position = e.Point;
                    GeoResult result = await rest.GetGeoResult(e.Point.Latitude.ToString() + ',' + e.Point.Longitude.ToString());
                    if (result != null)
                    {
                        origin = result.results[0].formatted_address;
                        setEntryStatus(result.results[0].formatted_address, "Home Address");
                        startPin.Address = result.results[0].formatted_address;
                    }
                    if (map.Pins.Contains(startPin))
                        map.Pins.Remove(startPin);
                    map.Pins.Add(startPin);
                    isHomeSet = 2;
                }

                mapMode = 0;

            };

            // Map Long clicked
            map.MapLongClicked += (sender, e) =>
            {
                var lat = e.Point.Latitude.ToString();
                var lng = e.Point.Longitude.ToString();
            };

        }

		async protected override void OnAppearing()
		{
            base.OnAppearing();
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
                if (isFirstLaunch)
                {
                    if (status == PermissionStatus.Granted)
                    {
                        if (Utils.IsLocationAvailable())
                        {
                            originPos = await Utils.GetPosition();
                            currentPos = originPos.Latitude + "," + originPos.Longitude;
                            origin = currentPos;
                            isHomeSet = 2;
                            map.MoveToRegion(MapSpan.FromCenterAndRadius(originPos, Distance.FromMeters(5000)));
                        }
                    }
                    else if (status != PermissionStatus.Unknown)
                    {
                        await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                    }
                    isFirstLaunch = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        void Go_Clicked(object sender, System.EventArgs e)
        {
            CalculateRoute();
        }

        void Home_Focused(object sender, EventArgs e)
        {
            entryAddress.Unfocus();
            Navigation.PushModalAsync(new LocationSelectionPage(this, true));
        }

        void Destination_Focused(object sender, EventArgs e)
        {
            destinationAddress.Unfocus();
            Navigation.PushModalAsync(new LocationSelectionPage(this, false));
        }

        void Driving_Clicked(object sender, System.EventArgs e)
        {
            clearStyles();
            mode = 0;
            btnDriving.BackgroundColor = Color.White;
            btnDriving.TextColor = Color.FromHex("2196F3");
            CalculateRoute();
        }

        void Walking_Clicked(object sender, System.EventArgs e)
        {
            clearStyles();
            mode = 1;
            btnWalking.BackgroundColor = Color.White;
            btnWalking.TextColor = Color.FromHex("2196F3");
            CalculateRoute();
        }

        void Bicycling_Clicked(object sender, System.EventArgs e)
        {
            clearStyles();
            mode = 2;
            btnBicycling.BackgroundColor = Color.White;
            btnBicycling.TextColor = Color.FromHex("2196F3");
            CalculateRoute();
        }

        void clearStyles()
        {
            btnDriving.BackgroundColor = Color.FromHex("2196F3");
            btnWalking.BackgroundColor = Color.FromHex("2196F3");
            btnBicycling.BackgroundColor = Color.FromHex("2196F3");
          
            btnDriving.TextColor = Color.White;
            btnWalking.TextColor = Color.White;
            btnBicycling.TextColor = Color.White;

        }

        void setEntryStatus(string text, string placeholder = "")
        {
            entryAddress.Text = text;

        }

        void setDestinationStatus(string text, string placeholder = "")
        {
            destinationAddress.Text = text;
        }

        async void CalculateRoute() 
        {
            map.Polylines.Clear();

            var line = new Xamarin.Forms.GoogleMaps.Polyline();
            line.StrokeColor = Color.Red;
            line.StrokeWidth = 10;

            string originParam = originPos.Latitude.ToString() + ',' + originPos.Longitude.ToString();
            string destinationParam = destinationPos.Latitude.ToString() + ',' + destinationPos.Longitude.ToString();
            if(isHomeSet == 1)
                originParam = origin;
            
            if (isDestinationSet == 1)
                destinationParam = destination;
            
            distanceLabel.Text = "";

            if (!string.IsNullOrEmpty(originParam) && !string.IsNullOrEmpty(destinationParam) && isHomeSet>0 && isDestinationSet>0)
            {
                var distanceResult = await rest.GetDistance(originParam, destinationParam, modes[mode]);
                if (distanceResult != null)
                {
                    if (distanceResult.rows[0].elements[0].distance != null)
                    {
                        string distance = distanceResult.rows[0].elements[0].distance.text;
                        string duration = distanceResult.rows[0].elements[0].duration.text;
                        distanceLabel.Text = $"Distance={distance}, Duration={duration}";
                    }
                }

                var result = await rest.GetDirection(originParam, destinationParam, modes[mode]);
                Bounds bounds = new Bounds(originPos, destinationPos);
                map.MoveToRegion(MapSpan.FromBounds(bounds));

                if (result != null)
                {
                    foreach (var route in result.routes)
                    {
                        foreach (var leg in route.legs)
                        {
                            foreach (var step in leg.steps)
                            {
                                var points = DecodePolyline(step.polyline.points);
                                foreach (var point in points)
                                {
                                    line.Positions.Add(point);
                                }
                            }
                        }
                    }
                    if (line.Positions.Count >= 2)
                        map.Polylines.Add(line);
                }
            }
            else
            {
                await this.DisplayAlert("Warning", "Please fill all the fields", "OK");
            }   
        }

    }
}

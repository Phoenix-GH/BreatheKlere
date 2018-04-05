using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using BreatheKlere.REST;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;

namespace BreatheKlere
{
    public partial class BreatheKlerePage : ContentPage
    {
        bool mapMode = false;
        bool isFirstLaunch;
        RESTService rest;

        public string origin, destination;
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
                            Position position = await Utils.GetPosition();
                            map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(5000)));
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
                await DisplayAlert("Error: ", ex.Message, "OK");
            }

            //Setting up the locations
            entryAddress.Text = origin;
            destinationAddress.Text = destination;
            if (map.Pins.Contains(startPin))
                map.Pins.Remove(startPin);
            startPin.Position = originPos;
            startPin.Address = origin;
            map.Pins.Add(startPin);

            if (map.Pins.Contains(endPin))
                map.Pins.Remove(endPin);
            endPin.Position = destinationPos;
            endPin.Address = destination;

            map.Pins.Add(endPin);
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

        async void Go_Clicked(object sender, System.EventArgs e)
        {
            map.Polylines.Clear();

            var line = new Xamarin.Forms.GoogleMaps.Polyline();
            line.StrokeColor = Color.Red;
            line.StrokeWidth = 10;

            string originParam = originPos.Latitude.ToString() + ',' + originPos.Longitude.ToString();
            string destinationParam = destinationPos.Latitude.ToString() + ',' + destinationPos.Longitude.ToString();
            
            if (!string.IsNullOrEmpty(originParam) && !string.IsNullOrEmpty(destinationParam))
            {
                var distanceResult = await rest.GetDistance(originParam, destinationParam);
                if (distanceResult != null)
                {
                    string distance = distanceResult.rows[0].elements[0].distance.text;
                    string duration = distanceResult.rows[0].elements[0].duration.text;
                    distanceLabel.Text = $"Distance={distance}, Duration={duration}";
                }

                var result = await rest.GetDirection(originParam, destinationParam);
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
                await this.DisplayAlert("Not found", "Please fill all the fields", "OK");
            }
        }

        void Home_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            entryAddress.Unfocus();
            Navigation.PushModalAsync(new LocationSelectionPage(this, true));
        }

        void Destination_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            destinationAddress.Unfocus();
            Navigation.PushModalAsync(new LocationSelectionPage(this, false));
        }
    }
}

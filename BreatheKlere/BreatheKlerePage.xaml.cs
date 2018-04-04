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
        byte selectionMode = 0;
        string origin, destination;
        Position originPos, destinationPos;
        RESTService rest;
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
                if (selectionMode == 2)
                {
                    destination = e.Point.Latitude.ToString() + ',' + e.Point.Longitude.ToString();
                    destinationPos = e.Point;
                    endPin.Position = e.Point;
                    setDestinationStatus("", "Pulling up location info...");
                    GeoResult result = await rest.getGeoResult(destination);
                    if (result!=null)
                    {
                        setDestinationStatus(result.results[0].formatted_address, "Destination Address");
                        endPin.Address = result.results[0].formatted_address;
                    }
                    map.Pins.Add(endPin);

                }
                else if(selectionMode == 1)
                {
                    originPos = e.Point;
                    origin = e.Point.Latitude.ToString() + ',' + e.Point.Longitude.ToString();
                    startPin.Position = e.Point;
                    setEntryStatus("", "Pulling up location info...");
                    GeoResult result = await rest.getGeoResult(origin);
                    if(result != null)
                    {
                        setEntryStatus(result.results[0].formatted_address, "Home Address");
                        startPin.Address = result.results[0].formatted_address;
                    }
                    map.Pins.Add(startPin);
                }
                selectionMode = 0;
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

                if (status == PermissionStatus.Granted)
                {
                    if (IsLocationAvailable())
                    {
                        var locator = CrossGeolocator.Current;
                        var pos = await locator.GetPositionAsync(TimeSpan.FromTicks(10000));
                        Position position = new Position(pos.Latitude, pos.Longitude);
                        map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMeters(5000)));
                    }
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error: ", ex.Message, "OK");
            }
		}

        public bool IsLocationAvailable()
        {
            if (!CrossGeolocator.IsSupported)
                return false;

            return CrossGeolocator.Current.IsGeolocationAvailable;
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

        void selectOrigin_Clicked (object sender, System.EventArgs e)
        {
            selectionMode = 1;
            setEntryStatus("", "Tap on the map to select location");

            if(map.Pins.Contains(startPin))
                map.Pins.Remove(startPin);
        }

        void selectDestination_Clicked(object sender, System.EventArgs e)
        {
            selectionMode = 2;
            setDestinationStatus("", "Tap on the map to select location");

            if(map.Pins.Contains(endPin))
                map.Pins.Remove(endPin);
        }

        async void Go_Clicked(object sender, System.EventArgs e)
        {
            map.Pins.Clear();
            map.Polylines.Clear();
            await GeocodeOrigin();
            await GeocodeDestination();

            var line = new Xamarin.Forms.GoogleMaps.Polyline();
            line.StrokeColor = Color.Red;
            line.StrokeWidth = 5;

            //Distance calculation
            var distanceResult = await rest.getDistance(entryAddress.Text, destinationAddress.Text);
            if (distanceResult !=null )
            {
                string distance = distanceResult.rows[0].elements[0].distance.text;
                string duration = distanceResult.rows[0].elements[0].duration.text;
                distanceLabel.Text = $"Distance={distance}, Duration={duration}";
            }


            var result = await rest.getDirection(entryAddress.Text, destinationAddress.Text);
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
                if(line.Positions.Count >= 2)
                    map.Polylines.Add(line);
            }
        }

        async Task<bool> GeocodeOrigin() 
        {
            GeoResult originResult = await rest.getGeoResult(entryAddress.Text);
            if (originResult != null)
            {
                if (originResult.results.Count > 0)
                {
                    double lat = originResult.results[0].geometry.location.lat;
                    double lng = originResult.results[0].geometry.location.lng;
                    originPos = new Position(lat, lng);

                    Pin pin = new Pin
                    {
                        Type = PinType.Place,
                        Label = originResult.results[0].formatted_address,
                        Address = originResult.results[0].formatted_address,
                        Position = originPos,
                    };
                    map.Pins.Add(pin);
                }
                else
                {
                    await this.DisplayAlert("Not found", "The original location does not exist", "Close");
                }
                return true;
            }
            else
            {
                await this.DisplayAlert("Not found", "Geocoder returns no results", "Close");
                return false;
            }
        }

        async Task<bool> GeocodeDestination() 
        {
            GeoResult destinationResult = await rest.getGeoResult(destinationAddress.Text);
            if (destinationResult != null)
            {
                if (destinationResult.results.Count > 0)
                {
                    double lat = destinationResult.results[0].geometry.location.lat;
                    double lng = destinationResult.results[0].geometry.location.lng;
                    destinationPos = new Position(lat, lng);
                    Pin pin = new Pin
                    {
                        Type = PinType.Place,
                        Label = destinationResult.results[0].formatted_address,
                        Address = destinationResult.results[0].formatted_address,
                        Position = destinationPos
                    };
                    map.Pins.Add(pin);
                }
                else
                {
                    await this.DisplayAlert("Not found", "The original location does not exist", "Close");
                }
                return true;
            }
            else
            {
                await this.DisplayAlert("Not found", "Geocoder returns no results", "Close");
                return false;
            }

        }

        void setEntryStatus(string text, string placeholder = "") {
            entryAddress.Text = text;
            entryAddress.Placeholder = placeholder;
        }

        void setDestinationStatus(string text, string placeholder = "")
        {
            destinationAddress.Text = text;
            destinationAddress.Placeholder = placeholder;
        }
    }
}

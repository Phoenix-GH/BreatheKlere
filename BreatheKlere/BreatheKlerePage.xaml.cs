using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using BreatheKlere.REST;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace BreatheKlere
{
    public partial class BreatheKlerePage : ContentPage
    {
        bool isDrawingBegan;
        string origin, destination;
        RESTService rest;
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
            isDrawingBegan = false;
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
            Pin startPin = new Pin
            {
                Type = PinType.SavedPin,
                Label = "Start Point",
            };
            Pin endPin = new Pin
            {
                Type = PinType.Generic,
                Label = "End Point",
            };
            // Map Clicked
            map.MapClicked += async (sender, e) =>
            {
                var lat = e.Point.Latitude.ToString();
                var lng = e.Point.Longitude.ToString();

                if(!isDrawingBegan)
                {
                    origin = lat + ',' + lng;
                    map.Pins.Clear();
                    map.Polygons.Clear();
                    startPin.Position = e.Point;
                    startPin.Address = origin;
                    map.Pins.Add(startPin);   

                }
                else
                {
                    destination = lat + ',' + lng;
                    var line = new Xamarin.Forms.GoogleMaps.Polyline();
                    line.StrokeColor = Color.Red;
                    line.StrokeWidth = 5;

                    endPin.Position = e.Point;
                    endPin.Address = destination;
                    map.Pins.Add(endPin);   

                    var result = await rest.getDirection(origin, destination);
                    map.Polylines.Clear();
                    if(result != null) 
                    {
                        foreach (var route in result.routes) 
                        {
                            line.Positions.Clear();
                            foreach(var leg in route.legs) 
                            {
                                foreach (var step in leg.steps)
                                {
                                    if(line.Positions.Count == 0)
                                    {
                                        line.Positions.Add(new Position(step.start_location.lat, step.start_location.lng));
                                    }
                                    line.Positions.Add(new Position(step.end_location.lat, step.end_location.lng));
                                }
                            }
                            map.Polylines.Add(line);
                        }
                    }

                }
                isDrawingBegan = !isDrawingBegan;

            };

            // Map Long clicked
            map.MapLongClicked += (sender, e) =>
            {
                var lat = e.Point.Latitude.ToString();
                var lng = e.Point.Longitude.ToString();
                //this.DisplayAlert("MapLongClicked", $"{lat}/{lng}", "CLOSE");
            };

            map.CameraChanged += (sender, args) =>
            {
                var p = args.Position;
                //labelStatus.Text = $"Lat={p.Target.Latitude:0.00}, Long={p.Target.Longitude:0.00}, Zoom={p.Zoom:0.00}, Bearing={p.Bearing:0.00}, Tilt={p.Tilt:0.00}";
            };

            // Geocode
            buttonGeocode.Clicked += async (sender, e) =>
            {
                
                GeoResult result = await rest.getGeoResult(entryAddress.Text);
                if(result != null)
                {
                    if (result.results.Count > 0)
                    {
                        double lat = result.results[0].geometry.location.lat;
                        double lng = result.results[0].geometry.location.lng;
                        var pos = new Position(lat, lng);
                        map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMeters(5000)));
                        map.Pins.Clear();
                        Pin pin = new Pin
                        {
                            Type = PinType.Place,
                            Label = result.results[0].formatted_address,
                            Address = result.results[0].formatted_address,
                            Position = pos,
                        };
                        map.Pins.Add(pin);
                    }
                    else
                    {
                        await this.DisplayAlert("Not found", "The location does not exist", "Close");
                    }
                }
                else
                {
                    await this.DisplayAlert("Not found", "Geocoder returns no results", "Close");
                }
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
	}
}

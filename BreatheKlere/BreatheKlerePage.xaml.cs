using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using BreatheKlere.REST;
using Plugin.Geolocator;
using System.Diagnostics;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace BreatheKlere
{
    public partial class BreatheKlerePage : ContentPage
    {
        public BreatheKlerePage()
        {
            InitializeComponent();
            // MapTypes
            var mapTypeValues = new List<MapType>();
            foreach (var mapType in Enum.GetValues(typeof(MapType)))
            {
                mapTypeValues.Add((MapType)mapType);
            }

            map.MapType = mapTypeValues[0];

            // MyLocationEnabled
            map.MyLocationEnabled = true;

            // IsTrafficEnabled
            map.IsTrafficEnabled = true;

            // IndoorEnabled
            map.IsIndoorEnabled = false;

            // CompassEnabled
            map.UiSettings.CompassEnabled = true;

            // RotateGesturesEnabled
            map.UiSettings.RotateGesturesEnabled = true;

            // MyLocationButtonEnabled
            map.UiSettings.MyLocationButtonEnabled = true;

            // IndoorLevelPickerEnabled
            map.UiSettings.IndoorLevelPickerEnabled = false;

            // ScrollGesturesEnabled
            map.UiSettings.ScrollGesturesEnabled = true;

            // TiltGesturesEnabled
            map.UiSettings.TiltGesturesEnabled = false;

            map.UiSettings.ZoomControlsEnabled = true;

            map.UiSettings.ZoomGesturesEnabled = true;

            // Map Clicked
            map.MapClicked += (sender, e) =>
            {
                var lat = e.Point.Latitude.ToString("0.000");
                var lng = e.Point.Longitude.ToString("0.000");
                //this.DisplayAlert("MapClicked", $"{lat}/{lng}", "CLOSE");
            };

            // Map Long clicked
            map.MapLongClicked += (sender, e) =>
            {
                var lat = e.Point.Latitude.ToString("0.000");
                var lng = e.Point.Longitude.ToString("0.000");
                //this.DisplayAlert("MapLongClicked", $"{lat}/{lng}", "CLOSE");
            };

            // Map MyLocationButton clicked
            //map.MyLocationButtonClicked += (sender, args) =>
            //{
            //    args.Handled = switchHandleMyLocationButton.IsToggled;
            //    if (switchHandleMyLocationButton.IsToggled)
            //    {
            //        this.DisplayAlert("MyLocationButtonClicked",
            //                     "If set MyLocationButtonClickedEventArgs.Handled = true then skip obtain current location",
            //                     "OK");
            //    }

            //};

            map.CameraChanged += (sender, args) =>
            {
                var p = args.Position;
                //labelStatus.Text = $"Lat={p.Target.Latitude:0.00}, Long={p.Target.Longitude:0.00}, Zoom={p.Zoom:0.00}, Bearing={p.Bearing:0.00}, Tilt={p.Tilt:0.00}";
            };

            // Geocode

            buttonGeocode.Clicked += async (sender, e) =>
            {
                RESTService rest = new RESTService();
                Result result = await rest.getGeoResult(entryAddress.Text);

                //var geocoder = new Xamarin.Forms.GoogleMaps.Geocoder();
                //var positions = await geocoder.GetPositionsForAddressAsync(entryAddress.Text);
                //if (positions.Count() > 0)
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

                await DisplayAlert("Error: ",ex.Message,"OK");
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

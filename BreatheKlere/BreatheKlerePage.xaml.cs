using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

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
                this.DisplayAlert("MapClicked", $"{lat}/{lng}", "CLOSE");
            };

            // Map Long clicked
            map.MapLongClicked += (sender, e) =>
            {
                var lat = e.Point.Latitude.ToString("0.000");
                var lng = e.Point.Longitude.ToString("0.000");
                this.DisplayAlert("MapLongClicked", $"{lat}/{lng}", "CLOSE");
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
                labelStatus.Text = $"Lat={p.Target.Latitude:0.00}, Long={p.Target.Longitude:0.00}, Zoom={p.Zoom:0.00}, Bearing={p.Bearing:0.00}, Tilt={p.Tilt:0.00}";
            };

            // Geocode
            buttonGeocode.Clicked += async (sender, e) =>
            {
                var geocoder = new Xamarin.Forms.GoogleMaps.Geocoder();
                var positions = await geocoder.GetPositionsForAddressAsync(entryAddress.Text);
                if (positions.Count() > 0)
                {
                    var pos = positions.First();
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pos, Distance.FromMeters(5000)));
                    var reg = map.VisibleRegion;
                    var format = "0.00";
                    labelStatus.Text = $"Center = {reg.Center.Latitude.ToString(format)}, {reg.Center.Longitude.ToString(format)}";
                }
                else
                {
                    await this.DisplayAlert("Not found", "Geocoder returns no results", "Close");
                }
            };

        }
    }
}

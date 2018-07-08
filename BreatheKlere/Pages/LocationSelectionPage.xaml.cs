/* Location Selection Modal */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BreatheKlere.REST;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace BreatheKlere
{
    public partial class LocationSelectionPage : ContentPage
    {
        BreatheKlerePage parent;
        Stopwatch timer;
        RESTService rest;
        bool isHomeSelected;
        List<string> recentSearchList;

        public LocationSelectionPage(BreatheKlerePage parent, bool isHomeSelected = true)
        {
            InitializeComponent();
            this.parent = parent;
            this.isHomeSelected = isHomeSelected;
            timer = new Stopwatch();
            rest = new RESTService();
            if (isHomeSelected)
                locationEntry.Placeholder = "Choose start point";
            recentSearchList = new List<string>();

            if (App.Current.Properties.ContainsKey("recentList"))
                recentSearchList = JsonConvert.DeserializeObject<List<string>>(App.Current.Properties["recentList"].ToString());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            foreach (var item in recentSearchList)
            {
                var cell = new TextCell()
                {
                    Text = item,
                };
                cell.Tapped += (sender, e) => {

                    if (isHomeSelected)
                    {
                        parent.origin = item;
                        parent.isHomeSet = 1;
                    }
                    else
                    {
                        parent.destination = item;
                        parent.isDestinationSet = 1;
                    }

                    Navigation.PopModalAsync();
                };
                recentList.Add(cell);
            }
            locationEntry.Focus();
        }

        async void Your_Location_Tapped(object sender, System.EventArgs e)
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
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
                    if (Utils.IsLocationAvailable())
                    {
                        Position position = await Utils.GetPosition();
                        parent.currentPos = position.Latitude + "," + position.Longitude;
                        if (isHomeSelected)
                        {
                            parent.originPos = position;
                            parent.origin = "Your location";
                            parent.isHomeSet = 2;
                        }
                        else
                        {
                            parent.destinationPos = position;
                            parent.destination = "Your location";
                            parent.isDestinationSet = 2;
                        }
                        await Navigation.PopModalAsync();
                    }
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void Map_Tapped(object sender, System.EventArgs e)
        {
            if (isHomeSelected)
                parent.mapMode = 1;
            else
                parent.mapMode = 2;
            Navigation.PopModalAsync();
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(locationEntry.Text))
            {
                timer.Restart();
                Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
                {
                    if (timer.ElapsedMilliseconds >= 1000)
                    {
                        GetPlaces(locationEntry.Text, parent.currentPos);
                        timer.Stop();
                    }
                    return false;
                });
            }
        }

        async Task<bool> GetPlaces(string locationName, string location = null)
        {
            locationList.Clear();

            Place result = await rest.GetPlaces(locationName, location);
            if (result != null)
            {
                if (result.predictions.Count > 0)
                {
                    foreach(var item in result.predictions) 
                    {
                        var cell = new TextCell()
                        {
                            Text = item.description,
                        };
                        cell.Tapped += (sender, e) => {
                            
                            if (isHomeSelected)
                            {
                                parent.origin = item.description;
                                parent.isHomeSet = 1;
                            }
                            else
                            {
                                parent.destination = item.description;
                                parent.isDestinationSet = 1;
                            }

                            recentSearchList.Add(item.description);
                            App.Current.Properties["recentList"] = JsonConvert.SerializeObject(recentSearchList);
                            Navigation.PopModalAsync();
                        };
                        locationList.Add(cell);

                    }
                    return true;
                }
                else
                {
                    Debug.WriteLine("Could not get info of home address");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine("Geocoder returns no results");
                return false;
            }

        }

      }
}

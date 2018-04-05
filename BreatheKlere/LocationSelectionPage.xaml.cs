using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BreatheKlere.REST;
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
        string[] listLondon = {"Big Ben", "Trafalgar Square", "Wembley Stadium", "Emirates", "Stamford Bridge", "Olympic Park", "Bank of England", "London City Airport"};
        public LocationSelectionPage(BreatheKlerePage parent, bool isHomeSelected = true)
        {
            InitializeComponent();
            this.parent = parent;
            this.isHomeSelected = isHomeSelected;
            timer = new Stopwatch();
            rest = new RESTService();
            for (var i = 0; i < listLondon.Length; i++) 
            {
                var cell = new TextCell
                {
                    Text = listLondon[i],
                };
                cell.Tapped += async (sender, e) =>
                {
                    
                    GeoResult result = await rest.GetGeoResult(((TextCell)sender).Text);
                    if (result != null)
                    {
                        if(result.results.Count > 0)
                        {
                            double lat = result.results[0].geometry.location.lat;
                            double lng = result.results[0].geometry.location.lng;
                            if (isHomeSelected)
                            {
                                parent.originPos = new Position(lat, lng);
                                parent.origin = result.results[0].formatted_address;
                            }
                            else
                            {
                                parent.destinationPos = new Position(lat, lng);
                                parent.destination = result.results[0].formatted_address;
                            }
                            await Navigation.PopModalAsync();
                        }
                    }
                };
                londonLocationList.Add(cell);
            }
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
                        if (isHomeSelected)
                        {
                            parent.originPos = position;
                            parent.origin = "Your location";
                        }
                        else
                        {
                            parent.destinationPos = position;
                            parent.destination = "Your location";
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
                        Debug.WriteLine("search progress");
                        GeoLocation(locationEntry.Text);
                        timer.Stop();
                    }
                    return false;
                });
            }
        }

        async Task<bool> GeoLocation(string location)
        {
            locationList.Clear();

            GeoResult result = await rest.GetGeoResult(location);
            if (result != null)
            {
                if (result.results.Count > 0)
                {
                    foreach(var item in result.results) 
                    {
                        var cell = new TextCell()
                        {
                            Text = item.formatted_address,
                        };
                        cell.Tapped += (sender, e) => {
                            double lat = item.geometry.location.lat;
                            double lng = item.geometry.location.lng;
                            if (isHomeSelected)
                            {
                                parent.originPos = new Position(lat, lng);
                                parent.origin = item.formatted_address;
                            }
                            else
                            {
                                parent.destinationPos = new Position(lat, lng);
                                parent.destination = item.formatted_address;
                            }
                            Navigation.PopModalAsync();
                        };
                        locationList.Add(cell);
                    }
                    return true;
                }
                else
                {
                    await this.DisplayAlert("Not found", "Could not get info of home address", "OK");
                    return false;
                }
            }
            else
            {
                await this.DisplayAlert("Not found", "Geocoder returns no results", "OK");
                return false;
            }
        }
    }
}

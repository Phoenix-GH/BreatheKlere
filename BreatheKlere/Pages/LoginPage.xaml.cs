using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BreatheKlere.REST;
using Xamarin.Forms;

namespace BreatheKlere
{
    public partial class LoginPage : ContentPage
    {
        RESTService rest;
        public LoginPage()
        {
            InitializeComponent();
            var genderList = new List<string>();
            genderList.Add("Y");
            genderList.Add("N");
            genderPicker.ItemsSource = genderList;
            rest = new RESTService();
        }

        async void OnLogin(object sender, System.EventArgs e)
        {
            if(genderPicker.SelectedIndex == 0)
            {
                await DisplayAlert("Warning", "Please select all fields", "OK");
                return;
            }
            var result = await rest.Register(nameEntry.Text, emailEntry.Text, "password", postcodeEntry.Text, genderPicker.SelectedItem.ToString());
            Debug.WriteLine(result.ToString());
            if(result!=null) {
                await Navigation.PushAsync(new BreatheKlerePage());
            }
            else {
                await DisplayAlert("Error", "Error on signing up", "OK");
            }
        } 

        void Handle_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            DisplayAlert("Notice", "Thank you for your feedback. This feature will be available soon.", "OK");
        }
    }
}

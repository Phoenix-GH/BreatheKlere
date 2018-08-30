using System.Collections.Generic;
using System.Diagnostics;
using BreatheKlere.REST;
using Xamarin.Forms;

namespace BreatheKlere
{
    public partial class RegistrationPage : ContentPage
    {
        RESTService rest;
        List<string> genderList;
        public RegistrationPage()
        {
            InitializeComponent();
            genderList = new List<string>();
            genderList.Add("Y");
            genderList.Add("N");
            foreach (var item in genderList)
            {
                userSegment.Children.Add(new SegmentedControl.FormsPlugin.Abstractions.SegmentedControlOption { Text = item });

            }
            rest = new RESTService();
        }

        async void OnRegistration(object sender, System.EventArgs e)
        {
            var result = await rest.Register(nameEntry.Text, emailEntry.Text, passwordEntry.Text, postcodeEntry.Text, userSegment.SelectedSegment.ToString(), "1234567");

            if (result != null)
            {
                if (!string.IsNullOrEmpty(result.deviceID))
                {
                    App.Current.Properties["isRegistered"] = true;
                    App.Current.Properties["DID"] = result.deviceID;
                    Debug.WriteLine("device -----", result.deviceID);
                    await Navigation.PushAsync(new BreatheKlerePage());
                }
                else if (!string.IsNullOrEmpty(result.error))
                    await DisplayAlert("Error", result.error, "OK");
                else
                    await DisplayAlert("Error", "Unknown error", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Error on signing up", "OK");
            }
        }

        void Handle_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            DisplayAlert("Notice", "Thank you for your feedback. This feature will be available soon.", "OK");
        }
    }
}

using System.Collections.Generic;
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
            //genderPicker.ItemsSource = genderList;
            foreach (var item in genderList)
            {
                userSegment.Children.Add(new SegmentedControl.FormsPlugin.Abstractions.SegmentedControlOption { Text = item });

            }
            rest = new RESTService();
        }

        async void OnRegistration(object sender, System.EventArgs e)
        {
            var result = await rest.Register(nameEntry.Text, emailEntry.Text, passwordEntry.Text, postcodeEntry.Text, userSegment.SelectedSegment.ToString());

            if (result != null)
            {
                App.Current.Properties["isRegistered"] = true;
                await Navigation.PushAsync(new BreatheKlerePage());

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

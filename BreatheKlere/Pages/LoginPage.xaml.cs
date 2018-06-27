using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace BreatheKlere
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            var genderList = new List<string>();
            genderList.Add("Male");
            genderList.Add("Female");
            genderPicker.ItemsSource = genderList;
        }

        void OnLogin(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new BreatheKlerePage());
        }    

        void OnForgotPassword(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new BreatheKlerePage());
        }    

        void Handle_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

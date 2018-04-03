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
        }

        void OnLogin(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new BreatheKlerePage());
        }    

        void OnForgotPassword(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new BreatheKlerePage());
        }    
    }
}

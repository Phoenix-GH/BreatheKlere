using System.Diagnostics;
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
            rest = new RESTService();
        }

        async void OnLogin(object sender, System.EventArgs e)
        {
            var result = await rest.Login(emailEntry.Text, passwordEntry.Text);
            Debug.WriteLine(result.ToString());
            if(result!=null) {
                await Navigation.PushAsync(new BreatheKlerePage());
            }
            else {
                await DisplayAlert("Error", "Error on signing up", "OK");
            }
        }

        void OnRegistration(object sender, System.EventArgs e)
        {
            Navigation.PushAsync(new RegistrationPage());
        }
    }
}

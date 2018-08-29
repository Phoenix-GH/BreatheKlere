using Xamarin.Forms;

namespace BreatheKlere
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //if(Current.Properties.ContainsKey("isRegistered"))
                //MainPage = new NavigationPage(new BreatheKlerePage());
            //else
                MainPage = new NavigationPage(new RegistrationPage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

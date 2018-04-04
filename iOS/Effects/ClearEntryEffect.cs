using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using UIKit;

[assembly: ResolutionGroupName("Effects")]
[assembly: ExportEffect(typeof(BreatheKlere.iOS.ClearEntryEffect), "ClearEntryEffect")]
namespace BreatheKlere.iOS
{
    public class ClearEntryEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            ConfigureControl();
        }

        protected override void OnDetached()
        {
        }

        private void ConfigureControl()
        {
            ((UITextField)Control).ClearButtonMode = UITextFieldViewMode.WhileEditing;
        }
    }
}
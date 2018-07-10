using System;
using BreatheKlere.iOS;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Picker), typeof(CustomPickerRenderer))]
namespace BreatheKlere.iOS
{
    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null || e.NewElement != null)
            {
                var customPicker = e.NewElement as CustomPicker;

                string placeholderColor = customPicker.PlaceholderColor;
                UIColor color = UIColor.FromRGB(GetRed(placeholderColor), GetGreen(placeholderColor), GetBlue(placeholderColor));

                var placeholderAttributes = new NSAttributedString(customPicker.Title, new UIStringAttributes()
                { ForegroundColor = color });

                Control.AttributedPlaceholder = placeholderAttributes;
            }
        }

        private float GetRed(string color)
        {
            Color c = Color.FromHex(color);
            return (float)c.R;
        }

        private float GetGreen(string color)
        {
            Color c = Color.FromHex(color);
            return (float)c.G;
        }

        private float GetBlue(string color)
        {
            Color c = Color.FromHex(color);
            return (float)c.B;
        }
    }
}

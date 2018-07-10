using System;
using BreatheKlere.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Picker), typeof(CustomPickerRenderer))]
namespace BreatheKlere.Droid
{
    public class CustomPickerRenderer : PickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            this.Control.Background = Forms.Context.GetDrawable(Resource.Drawable.CustomPickerBackground);
            Control?.SetPadding(20, 20, 20, 20);
            if (e.OldElement != null || e.NewElement != null)
            {
                var customPicker = e.NewElement as CustomPicker;
                Control.TextSize *= (customPicker.FontSize * 0.01f);
                Control.SetHintTextColor(Android.Graphics.Color.ParseColor(customPicker.PlaceholderColor));
            }
        }
    }
}

using System;
using Xamarin.Forms;

namespace BreatheKlere
{
    public class CustomPicker : Picker
    {
        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(Int32), typeof(CustomPicker), 24, BindingMode.TwoWay);

        public Int32 FontSize
        {
            set { SetValue(FontSizeProperty, value); }
            get { return (Int32)GetValue(FontSizeProperty); }
        }

        public static BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(nameof(PlaceholderColor), typeof(string), typeof(CustomPicker), "#CCCCCC", BindingMode.TwoWay);

        public string PlaceholderColor
        {
            get { return (string)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }
    }
}

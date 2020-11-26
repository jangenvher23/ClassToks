using System;
using System.Diagnostics;
using System.Globalization;
using Xamarin.Forms;

namespace ClassToksV2
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine($"Convert: {value.ToString()}");
            if (value != null && value is string && (string)value != "")
                return Color.FromHex(value.ToString());
            else
                return Color.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.WriteLine($"Convert Back: {value.ToString()}");
            Color c = (Color)value;
            return c.ToHex();
        }
    }
}
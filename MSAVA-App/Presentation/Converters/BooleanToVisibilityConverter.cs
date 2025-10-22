using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace MSAVA_App.Presentation.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool flag = value is bool b && b;
            if (Invert) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility v)
            {
                var result = v == Visibility.Visible;
                return Invert ? !result : result;
            }
            return false;
        }
    }
}

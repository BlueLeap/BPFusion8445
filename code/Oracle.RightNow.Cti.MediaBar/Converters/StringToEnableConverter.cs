using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace Oracle.RightNow.Cti.MediaBar.Converters
{
    public class StringToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var stringvalue = (string)value;
                return stringvalue.ToLower().Equals("no") ? false : true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}

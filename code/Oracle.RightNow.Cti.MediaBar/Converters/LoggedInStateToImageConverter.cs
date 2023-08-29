using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Oracle.RightNow.Cti.MediaBar.Converters {
    public class LoggedInStateToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool) {
                var isLoggedIn = (bool)value;

                return isLoggedIn
                    ? "/Oracle.RightNow.Cti.Mediabar;component/Images/mediabar.disconnect.png"
                    : "/Oracle.RightNow.Cti.Mediabar;component/Images/mediabar.connect.png";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}

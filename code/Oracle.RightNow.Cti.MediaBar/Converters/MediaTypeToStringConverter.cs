using Oracle.RightNow.Cti.MediaBar.Properties;
using Oracle.RightNow.Cti.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Oracle.RightNow.Cti.MediaBar.Converters {
    public class MediaTypeToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string result = Resources.MediaTypeUnknown;
            if (value is MediaType) {
                var type = (MediaType)value;

                switch (type) {
                    case MediaType.Voice:
                        result = Resources.MediaTypeVoice;
                        break;
                    case MediaType.Email:
                        result = Resources.MediaTypeEmail;
                        break;
                    case MediaType.Fax:
                        break;
                    case MediaType.Chat:
                        result = Resources.MediaTypeChat;
                        break;
                    case MediaType.Web:
                        result = Resources.MediaTypeWeb;
                        break;
                    case MediaType.Social:
                        break;
                    case MediaType.Sms:
                        break;
                    case MediaType.Generic:
                        break;
                    default:
                        break;
                }
            }

            return result.ToLower();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

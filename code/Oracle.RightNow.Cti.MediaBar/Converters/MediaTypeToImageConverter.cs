using System;
using System.Globalization;
using System.Windows.Data;
using Oracle.RightNow.Cti.MediaBar.Properties;
using Oracle.RightNow.Cti.Model;

namespace Oracle.RightNow.Cti.MediaBar.Converters {
    public class MediaTypeToImageConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var interaction = value as IInteraction;

            var result = string.Empty;

            if (interaction != null) {
                switch (interaction.Type)
                {
                    case MediaType.Voice:
                        result = Resources.MediaTypeVoiceImageUri;
                        break;
                    case MediaType.Email:
                        result = Resources.MediaTypeEmailImageUri;
                        break;
                    case MediaType.Fax:
                        break;
                    case MediaType.Chat:
                        break;
                    case MediaType.Web:
                        result = Resources.MediaTypeWebImageUri;
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

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}
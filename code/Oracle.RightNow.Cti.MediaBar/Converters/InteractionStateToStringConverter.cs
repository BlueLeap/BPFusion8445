using System;
using System.Globalization;
using System.Windows.Data;
using Oracle.RightNow.Cti.MediaBar.Properties;
using Oracle.RightNow.Cti.Model;

namespace Oracle.RightNow.Cti.MediaBar.Converters {
    public class InteractionStateToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var result = string.Empty;

            if (value is InteractionState) {
                var state = (InteractionState)value;

                switch (state) {
                    case InteractionState.Ringing:
                        result = Resources.InteractionStateRinging;
                        break;
                    case InteractionState.Active:
                        result = Resources.InteractionStateActive;
                        break;
                    case InteractionState.Held:
                        result = Resources.InteractionStateHeld;
                        break;
                    case InteractionState.Disconnected:
                        result = Resources.InteractionStateDisconnected;
                        break;
                    case InteractionState.Dialing:
                        result = Resources.InteractionStateDialing;
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using Oracle.RightNow.Cti.Model;

namespace Oracle.RightNow.Cti.MediaBar.Converters {
    public class AgentStateToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var state = value as AgentState;
            
            var color = Colors.Gray;

            if (state != null)
                switch (state.SwitchMode)
                {
                    case AgentSwitchMode.Dynamic:
                    case AgentSwitchMode.NotReady:
                        color = Colors.Red;
                        break;
                    case AgentSwitchMode.LoggedIn:
                    case AgentSwitchMode.Ready:
                        color = Colors.YellowGreen;
                        break;
                    case AgentSwitchMode.WrapUp:
                    case AgentSwitchMode.Connecting:
                    case AgentSwitchMode.Talking:
                        color = Colors.Orange;
                        break;
                    case AgentSwitchMode.LoggedOut:
                        break;
                    case AgentSwitchMode.Reserved:
                        break;
                    case AgentSwitchMode.NewReason:
                        break;
                    case AgentSwitchMode.Hold:
                        break;
                    default:
                        break;
                }
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }
}
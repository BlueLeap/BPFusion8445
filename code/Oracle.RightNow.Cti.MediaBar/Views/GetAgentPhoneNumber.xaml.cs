using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Oracle.RightNow.Cti.MediaBar.ViewModels;

namespace Oracle.RightNow.Cti.MediaBar.Views
{
    /// <summary>
    /// Interaction logic for GetAgentPhoneNumber.xaml
    /// </summary>
    public partial class GetAgentPhoneNumber : Window
    {
        public GetAgentPhoneNumber()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, txtPhoneNumber);
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,+]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public GetAgentPhoneNumberViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }
    }
}

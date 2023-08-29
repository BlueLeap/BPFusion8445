using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Text.RegularExpressions;
namespace Oracle.RightNow.Cti.MediaBar.Views
{
    /// <summary>
    /// Interaction logic for SmsSenderView.xaml
    /// </summary>
    public partial class SmsSenderView : Window
    {
        public SmsSenderView()
        {
            InitializeComponent();
            FocusManager.SetFocusedElement(this, txtMessage);
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,+]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public SmsSenderViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }
    }
}

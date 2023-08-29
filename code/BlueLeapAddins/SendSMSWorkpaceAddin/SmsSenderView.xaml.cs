using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows;

namespace BlueLeap.Addins.SMS
{
    /// <summary>
    /// Interaction logic for SmsSenderView.xaml
    /// </summary>
    /// 
    public partial class SmsSenderView : Window
    {
        public SmsSenderView()
        {
            InitializeComponent();
            //FocusManager.SetFocusedElement(this, txtMessage);
        }

        //private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        //{
        //    Regex regex = new Regex("[^0-9,+]+");
        //    e.Handled = regex.IsMatch(e.Text);
        //}

        public SmsSenderViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }
    }
}

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
    /// Interaction logic for CreateCampaign.xaml
    /// </summary>
    public partial class CreateCampaign : UserControl
    {
        public CreateCampaign()
        {
            InitializeComponent();
            cboMergeFieldsList.Text = "Fetching Fields.Please wait.";
            cboMergeFieldsList.SelectedIndex = 0;
        }
        public CreateCampaignViewModel ViewModel
        {
            set
            {
                this.DataContext = value;
            }
        }

        private void ComboBoxItem_Selected(object sender, RoutedEventArgs e)
        {

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cboMergeFieldsList.SelectedValue != null)
            {
                var insertText = String.Format("[{0}]", cboMergeFieldsList.SelectedValue);
                var selectionIndex = txtMessageBody.SelectionStart;
                txtMessageBody.Text = txtMessageBody.Text.Insert(selectionIndex, insertText);
                txtMessageBody.SelectionStart = selectionIndex + insertText.Length;
            }
        }
    }
}

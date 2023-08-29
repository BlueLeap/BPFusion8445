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
using Oracle.RightNow.Cti.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Oracle.RightNow.Cti.MediaBar.ViewModels;
using System.Threading.Tasks;
using System.Windows.Threading;
using Oracle.RightNow.Cti.MediaBar.Helpers;
using Oracle.RightNow.Cti.AddIn;

/*
 * Not Follwing MVVM Patters here.
 * This file needs to be cleaned up to follow the better MVVM model.
 */

namespace Oracle.RightNow.Cti.MediaBar.Views
{
    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
    /// <summary>
    /// Interaction logic for ManageCampaign.xaml
    /// </summary>
    public partial class ManageCampaign : UserControl, INotifyPropertyChanged
    {
        private int AgentID;
        private StaffAccountInfo staffAccount;
        private Dictionary<string, string> mergeFields;
        private RightNowObjectProvider _objectProvider;
        public List<SMSCampaignModel> ContextCampaign;
        private ICollectionView _dataGridCollection;
        private string _filterString;
        int pageIndex = 1;
        private int numberOfRecPerPage;
        //To check the paging direction according to use selection.
        private enum PagingMode
        { First = 1, Next = 2, Previous = 3, Last = 4, PageCountChange = 5 };

        KeyValuePair<int, string> _selectedReport;
        public KeyValuePair<int, string> SelectedReport
        {
            get
            {
                return _selectedReport;
            }
            set
            {
                _selectedReport = value;
            }
        }
       
        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                NotifyPropertyChanged("FilterString");
                FilterCollection();
            }
        }

        private void FilterCollection()
        {
            if (_dataGridCollection != null)
            {
                _dataGridCollection.Refresh();
            }
        }


        public ICollectionView DataGridCollection
        {
            get { return _dataGridCollection; }
            set { _dataGridCollection = value; NotifyPropertyChanged("DataGridCollection"); }
        }

        public ManageCampaign(RightNowObjectProvider provider, Dictionary<string, string> fields, StaffAccountInfo sAccount, int AID)
        {
            InitializeComponent();
            gridCampaignlist.Visibility = Visibility.Hidden;
            lblLoading.Visibility = Visibility.Visible;
            //int count = ContextCampaign.Take(numberOfRecPerPage).Count();
            //lblpageInformation.Content = count + " of " + ContextCampaign.Count;
            ContextCampaign = new List<SMSCampaignModel>();
            cbNumberOfRecords.Items.Add("10");
            cbNumberOfRecords.Items.Add("20");
            cbNumberOfRecords.Items.Add("30");
            cbNumberOfRecords.Items.Add("50");
            cbNumberOfRecords.Items.Add("100");
            cbNumberOfRecords.SelectedItem = 10;
            gridCampaignlist.Visibility = Visibility.Visible;
            lblLoading.Visibility = Visibility.Hidden;

            AgentID = AID;
            mergeFields = fields;
            staffAccount = sAccount;
            ContextCampaign = new List<SMSCampaignModel>();

            _objectProvider = provider;
            
           

            ComboboxItem item1 = new ComboboxItem();
            item1.Text = "Manage Campaigns";
            item1.Value = MediaBarAddIn._ManageCampaignReportID;
            cboReportList.Items.Add(item1);
            cboReportList.SelectedIndex = 0;

        }

        public bool Filter(object obj)
        {
            var data = obj as SMSCampaignModel;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(_filterString))
                {
                    return data.CampaignName.Contains(_filterString) || data.MessageBody.Contains(_filterString);
                }
                return true;
            }
            return false;
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        private string ValidateCampaignDialog(CreateCampaignViewModel dialog)
        {
            string result = string.Empty;
            string missingFields = string.Empty;
            if (String.IsNullOrEmpty(dialog.CampaignName))
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Message Campaign Name";
                }
                else
                {
                    missingFields = missingFields + "," + "Message Campaign Name";
                }
            }

            if (String.IsNullOrEmpty(dialog.UserInput))
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Message Body";
                }
                else
                {
                    missingFields = missingFields + "," + "Message Body";
                }
            }

            if (dialog._ContactListIDInt == 0)
            {
                if (missingFields == string.Empty)
                {
                    missingFields = "Input ID (Try clicking the Fetch Button)";
                }
                else
                {
                    missingFields = missingFields + "," + "Input ID (Try clicking the Fetch Button)";
                }
            }

            if (missingFields != string.Empty)
            {
                result = "Missing field(s): " + missingFields;
            }

            return result;
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SMSCampaignModel registrationRow = gridCampaignlist.SelectedValue as SMSCampaignModel;
                DateTime dt = _objectProvider.GetCampaignRunTime(registrationRow.ID);

                if (!dt.Equals(DateTime.MinValue))
                {
                    registrationRow.RunTime = dt;
                }

                var dialog = new Window();
                dialog.ShowInTaskbar = false;
                dialog.Width = 570;
                dialog.Height = 550;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                dialog.ResizeMode = ResizeMode.CanResizeWithGrip;
                dialog.Title = "Update Campaign";
                var CCdialog = new CreateCampaign();

                ConsoleWindowHelper.SetupOwner(dialog);
                //IList<string> contactlist = _objectProvider.GetContactList();
                IList<string> contactlist = null;
                CCdialog.DataContext = new CreateCampaignViewModel((result,temp) =>
                {
                    if (result)
                    {
                        CreateCampaignViewModel model = (CreateCampaignViewModel)CCdialog.DataContext;
                        string validationResult = ValidateCampaignDialog(model);
                        if (validationResult == string.Empty)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                DateTime runTime = DateTime.MinValue;
                                try
                                {
                                    if (model.ScheduleOption == CampaignScheduleOption.Later)
                                    {
                                        runTime = model.RunTime.ToUniversalTime();// DateTime.ParseExact(model.RunTime.ToString(), "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture).ToUniversalTime();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Logger.Log.Debug("Error while converting dates", ex);
                                }

                                //DateTime.MinValue is used as a sentinal for Immediately.
                                if (_objectProvider.CreateUpdateSMSCampaign(model.CampaignName, runTime, model.UserInput, model._ContactListIDInt, model.OptOutOverride, model.AlphaNumericHeader, AgentID, true, temp.ID))
                                {
                                    Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Background,
                                    new Action(() =>
                                    {
                                        ComboboxItem item = (ComboboxItem)cboReportList.SelectedItem;
                                        gridCampaignlist.Visibility = Visibility.Hidden;
                                        lblLoading.Visibility = Visibility.Visible;
                                        lblLoading.Content = "Campaign Updated Successfully. Reloading data.";
                                        Task.Factory.StartNew(() =>
                                        {
                                            ContextCampaign = _objectProvider.GetCampaigns(Convert.ToInt32(item.Value));
                                            Application.Current.Dispatcher.BeginInvoke(
                                                           DispatcherPriority.Background,
                                                           new Action(() =>
                                                           {
                                                               if (ContextCampaign.Count != 0)
                                                               {
                                                                   gridCampaignlist.Visibility = Visibility.Visible;
                                                                   lblLoading.Visibility = Visibility.Hidden;
                                                                   //cbNumberOfRecords.SelectedItem = 10;
                                                                   DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Take(numberOfRecPerPage));
                                                                   DataGridCollection.Filter = new Predicate<object>(Filter);
                                                                   //gridCampaignlist.ItemsSource = ContextCampaign.Take(numberOfRecPerPage);
                                                                   
                                                               } else
                                                               {
                                                                   lblLoading.Content = "There are no items to show.";
                                                               }
                                                               int count = ContextCampaign.Take(numberOfRecPerPage).Count();
                                                               lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                                                           }));
                                        });
                                        //MessageBox.Show(Messages.MessageCampaignEditedSuccess, Messages.MessageBoxTitle);
                                    })
                                    );
                                }
                                else
                                {
                                    Application.Current.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Background,
                                    new Action(() =>
                                    {
                                        MessageBox.Show(Messages.MessageCampaignUpdateError, Messages.MessageBoxTitle);
                                    })
                                    );
                                }
                            });
                            dialog.Close();
                        }
                        else
                        {
                            CCdialog.errLabel.Content = validationResult;
                            CCdialog.errLabel.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        dialog.Close();
                    }
                }, contactlist, mergeFields, _objectProvider, staffAccount, AgentID ,registrationRow, "Update SMS Campaign");

                dialog.Content = CCdialog;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void gridCampaignlist_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if((e.PropertyName == "CampaignSenderID") || (e.PropertyName == "Total") || (e.PropertyName == "isQueued") || (e.PropertyName == "OptOut")
                || (e.PropertyName == "ListID") || (e.PropertyName == "StartTime") || (e.PropertyName == "FinishTime")  || (e.PropertyName == "EditVisiblity")
                || (e.PropertyName == "CancelVisiblity") || (e.PropertyName == "StopVisiblity"))
            {
                e.Cancel = true;
            }
        }

        private void gridCampaignlist_AutoGeneratedColumns(object sender, EventArgs e)
        {
        }

        private void cboReportList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboboxItem item = (ComboboxItem)cboReportList.SelectedItem;
            gridCampaignlist.Visibility = Visibility.Hidden;
            lblLoading.Visibility = Visibility.Visible;
            Task.Factory.StartNew(() =>
            {

                ContextCampaign = _objectProvider.GetCampaigns(Convert.ToInt32(item.Value));
                Application.Current.Dispatcher.BeginInvoke(
                               DispatcherPriority.Background,
                               new Action(() =>
                               {
                                   if (ContextCampaign.Count != 0)
                                   {
                                       gridCampaignlist.Visibility = Visibility.Visible;
                                       lblLoading.Visibility = Visibility.Hidden;
                                       //cbNumberOfRecords.SelectedItem = 10;
                                       DataGridCollection = null;
                                       DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Take(numberOfRecPerPage));
                                       DataGridCollection.Filter = new Predicate<object>(Filter);
                                       //gridCampaignlist.ItemsSource = ContextCampaign.Take(numberOfRecPerPage);
                                   } else
                                   {
                                       lblLoading.Content = "There are no items to show.";
                                   }
                                   int count = ContextCampaign.Take(numberOfRecPerPage).Count();
                                   lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                               }));
            });
        }

        private void btnFirst_Click(object sender, System.EventArgs e)
        {
            Navigate((int)PagingMode.First);
        }

        private void btnNext_Click(object sender, System.EventArgs e)
        {
            Navigate((int)PagingMode.Next);

        }

        private void btnPrev_Click(object sender, System.EventArgs e)
        {
            Navigate((int)PagingMode.Previous);

        }

        private void btnLast_Click(object sender, System.EventArgs e)
        {
            Navigate((int)PagingMode.Last);
        }

        private void cbNumberOfRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Navigate((int)PagingMode.PageCountChange);
        }

        private void Navigate(int mode)
        {
            int count;
            switch (mode)
            {
                case (int)PagingMode.Next:
                    btnPrev.IsEnabled = true;
                    btnFirst.IsEnabled = true;
                    if (ContextCampaign.Count >= (pageIndex * numberOfRecPerPage))
                    {
                        if (ContextCampaign.Skip(pageIndex *
                        numberOfRecPerPage).Take(numberOfRecPerPage).Count() == 0)
                        {
                            //gridCampaignlist.ItemsSource = null;
                            //gridCampaignlist.ItemsSource = ContextCampaign.Skip((pageIndex *
                            //numberOfRecPerPage) - numberOfRecPerPage).Take(numberOfRecPerPage);
                            DataGridCollection = null;
                            DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Skip((pageIndex *
                            numberOfRecPerPage) - numberOfRecPerPage).Take(numberOfRecPerPage));
                            count = (pageIndex * numberOfRecPerPage) +
                            (ContextCampaign.Skip(pageIndex *
                            numberOfRecPerPage).Take(numberOfRecPerPage)).Count();
                        }
                        else
                        {
                            //gridCampaignlist.ItemsSource = null;
                            //gridCampaignlist.ItemsSource = ContextCampaign.Skip(pageIndex *
                            //numberOfRecPerPage).Take(numberOfRecPerPage);
                            DataGridCollection = null;
                            DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Skip(pageIndex *
                            numberOfRecPerPage).Take(numberOfRecPerPage));
                            count = (pageIndex * numberOfRecPerPage) +
                            (ContextCampaign.Skip(pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage)).Count();
                            pageIndex++;
                        }

                        lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                    }

                    else
                    {
                        btnNext.IsEnabled = false;
                        btnLast.IsEnabled = false;
                    }

                    break;
                case (int)PagingMode.Previous:
                    btnNext.IsEnabled = true;
                    btnLast.IsEnabled = true;
                    if (pageIndex > 1)
                    {
                        pageIndex -= 1;
                        //gridCampaignlist.ItemsSource = null;
                        if (pageIndex == 1)
                        {
                            //gridCampaignlist.ItemsSource = ContextCampaign.Take(numberOfRecPerPage);
                            DataGridCollection = null;
                            DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Take(numberOfRecPerPage));
                            count = ContextCampaign.Take(numberOfRecPerPage).Count();
                            lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                        }
                        else
                        {
                            //gridCampaignlist.ItemsSource = ContextCampaign.Skip
                            //(pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage);
                            DataGridCollection = null;
                            DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Skip
                            (pageIndex * numberOfRecPerPage).Take(numberOfRecPerPage));
                            count = Math.Min(pageIndex * numberOfRecPerPage, ContextCampaign.Count);
                            lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                        }
                    }
                    else
                    {
                        btnPrev.IsEnabled = false;
                        btnFirst.IsEnabled = false;
                    }
                    break;

                case (int)PagingMode.First:
                    pageIndex = 2;
                    Navigate((int)PagingMode.Previous);
                    break;
                case (int)PagingMode.Last:
                    pageIndex = (ContextCampaign.Count / numberOfRecPerPage);
                    Navigate((int)PagingMode.Next);
                    break;

                case (int)PagingMode.PageCountChange:
                    pageIndex = 1;
                    numberOfRecPerPage = Convert.ToInt32(cbNumberOfRecords.SelectedItem);
                    //gridCampaignlist.ItemsSource = null;
                    DataGridCollection = null;
                    //gridCampaignlist.ItemsSource = ContextCampaign.Take(numberOfRecPerPage);
                    DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Take(numberOfRecPerPage));
                    count = (ContextCampaign.Take(numberOfRecPerPage)).Count();
                    lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                    btnNext.IsEnabled = true;
                    btnLast.IsEnabled = true;
                    btnPrev.IsEnabled = true;
                    btnFirst.IsEnabled = true;
                    break;
            }
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            SMSCampaignModel registrationRow = gridCampaignlist.SelectedValue as SMSCampaignModel;
            if (MessageBox.Show(String.Format(Messages.CancelCampaignMessage, registrationRow.ID, registrationRow.CampaignName), Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Task.Factory.StartNew(() =>
                {
                    if (_objectProvider.CancelStopCampaign(registrationRow.ID))
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            //MessageBox.Show(Messages.SuccessfulUpdateCancelCampaignMessage, Messages.MessageBoxTitle);
                            ComboboxItem item = (ComboboxItem)cboReportList.SelectedItem;
                            gridCampaignlist.Visibility = Visibility.Hidden;
                            lblLoading.Visibility = Visibility.Visible;
                            lblLoading.Content = "Campaign Cancelled Successfully. Reloading data.";
                            Task.Factory.StartNew(() =>
                            {
                                ContextCampaign = _objectProvider.GetCampaigns(Convert.ToInt32(item.Value));
                                Application.Current.Dispatcher.BeginInvoke(
                                               DispatcherPriority.Background,
                                               new Action(() =>
                                               {
                                                   if (ContextCampaign.Count != 0)
                                                   {
                                                       gridCampaignlist.Visibility = Visibility.Visible;
                                                       lblLoading.Visibility = Visibility.Hidden;
                                                       //cbNumberOfRecords.SelectedItem = 10;
                                                       DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Take(numberOfRecPerPage));
                                                       DataGridCollection.Filter = new Predicate<object>(Filter);
                                                       //gridCampaignlist.ItemsSource = ContextCampaign.Take(numberOfRecPerPage);

                                                   }
                                                   else
                                                   {
                                                       lblLoading.Content = "There are no items to show.";
                                                   }
                                                   int count = ContextCampaign.Take(numberOfRecPerPage).Count();
                                                   lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                                               }));
                            });
                        }));
                    }
                });
            }
        }

        private void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            SMSCampaignModel registrationRow = gridCampaignlist.SelectedValue as SMSCampaignModel;
            if (MessageBox.Show(String.Format(Messages.StopCampaignMessage, registrationRow.ID, registrationRow.CampaignName), Messages.MessageBoxTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Task.Factory.StartNew(() =>
                {
                    if (_objectProvider.CancelStopCampaign(registrationRow.ID, false))
                    {
                        Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background,
                        new Action(() =>
                        {
                            //MessageBox.Show(Messages.SuccessfulUpdateCancelCampaignMessage, Messages.MessageBoxTitle);
                            ComboboxItem item = (ComboboxItem)cboReportList.SelectedItem;
                            gridCampaignlist.Visibility = Visibility.Hidden;
                            lblLoading.Visibility = Visibility.Visible;
                            lblLoading.Content = "Campaign stopped Successfully. Reloading data.";
                            Task.Factory.StartNew(() =>
                            {
                                ContextCampaign = _objectProvider.GetCampaigns(Convert.ToInt32(item.Value));
                                Application.Current.Dispatcher.BeginInvoke(
                                               DispatcherPriority.Background,
                                               new Action(() =>
                                               {
                                                   if (ContextCampaign.Count != 0)
                                                   {
                                                       gridCampaignlist.Visibility = Visibility.Visible;
                                                       lblLoading.Visibility = Visibility.Hidden;
                                                       //cbNumberOfRecords.SelectedItem = 10;
                                                       DataGridCollection = CollectionViewSource.GetDefaultView(ContextCampaign.Take(numberOfRecPerPage));
                                                       DataGridCollection.Filter = new Predicate<object>(Filter);
                                                       //gridCampaignlist.ItemsSource = ContextCampaign.Take(numberOfRecPerPage);

                                                   }
                                                   else
                                                   {
                                                       lblLoading.Content = "There are no items to show.";
                                                   }
                                                   int count = ContextCampaign.Take(numberOfRecPerPage).Count();
                                                   lblpageInformation.Content = count + " of " + ContextCampaign.Count;
                                               }));
                            });
                        }));
                    }
                });
            }
        }
    }
   
}

using System;
using System.Windows;
using BlueLeap.Addins.SMS;
using RightNow.AddIns.AddInViews;
using Oracle.RightNow.Cti;
using Oracle.RightNow.Cti.Logger;
using Oracle.RightNow.Cti.Model;
using BlueLeap.Helpers;
using System.Text.RegularExpressions;
using PhoneNumbers;
namespace BlueLeapAddins.SMSAddin
{
    public partial class SMSSendUserControl : System.Windows.Forms.UserControl
    {
        public delegate void SMSSendHandler();
        public event SMSSendHandler SMSSendClicked;
        private IRecordContext _recContext;
        private IGlobalContext RightNowGlobalContext { get; set; }
        private RightNowObjectProvider _objectProvider;
        private PhoneNumberUtil util;
        public SMSSendUserControl(IRecordContext _contect, IGlobalContext _gcontext)
        {
            _recContext = _contect;
            RightNowGlobalContext = _gcontext;
            _objectProvider = new RightNowObjectProvider(_gcontext);
            util = PhoneNumberUtil.GetInstance();
            InitializeComponent();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            IContact incontact = _recContext.GetWorkspaceRecord(RightNow.AddIns.Common.WorkspaceRecordType.Contact) as IContact;
            bool OptOutOverRide = false;
            if (incontact != null && _objectProvider.IsContactHasOptOutFlagSet(incontact.ID.ToString()))
            {
                OptOutOverRide = true;
            }
            Contact destContact = null;

            if (incontact != null)
            {
                destContact = new Contact
                {
                    Id = incontact.ID.ToString(),
                    Number = incontact.PhMobile,
                };
            }

            var SMSDialog = new SmsSenderView();
            SMSDialog.ResizeMode = ResizeMode.CanResizeWithGrip;
            ConsoleWindowHelper.SetupOwner(SMSDialog);
            SMSDialog.DataContext = new SmsSenderViewModel(destContact, (result, contact) =>
            {
                SmsSenderViewModel model = (SmsSenderViewModel)SMSDialog.DataContext;
                if (contact != null)
                {
                    if (OptOutOverRide && !model.OptOutOverride)
                    {
                        model.ShowErrorLabel = true;
                        SMSDialog.errLabel.Content = "Cannot send message as contact is set to SMS opt out.";
                        return;
                    }
                    if (String.IsNullOrEmpty(SMSDialog.txtDestinationNumber.Text))
                    {
                        model.ShowErrorLabel = true;
                        SMSDialog.errLabel.Content = "Please enter the destination number.";
                        return;
                    }
                    else if (String.IsNullOrEmpty(SMSDialog.txtMessage.Text))
                    {
                        model.ShowErrorLabel = true;
                        SMSDialog.errLabel.Content = "Please enter some message.";
                        return;
                    }

                    Logger.Log.Debug("fnSendSMS ** call fnSendSMS() **");
                    try
                    {
                        string rawDestContactNumber = "";
                        String processedContactDestinationNumber = ""; bool isLandlineNumber;
                        if (destContact != null)
                        {
                            rawDestContactNumber = destContact.Number.Trim();
                        }
                        else
                        {
                            rawDestContactNumber = SMSDialog.txtDestinationNumber.Text.Trim();
                        }

                        if (checkNumberInInternationalFormat(rawDestContactNumber) && checkForLockedNumber(rawDestContactNumber))
                        {
                            MessageBox.Show(String.Format(Messages.MessageNoPermissionToUseCountryCode, rawDestContactNumber.Substring(1, 2)), Messages.MessageBoxTitle);
                        }
                        else
                        {
                            DoMagicOnNumber(rawDestContactNumber, out processedContactDestinationNumber, out isLandlineNumber);

                            if (processedContactDestinationNumber == null)
                            {
                                MessageBox.Show(String.Format(Messages.MessageCannotFormatNumber, rawDestContactNumber), Messages.MessageBoxTitle);
                                return;
                            }
                            else
                            {
                                if (isLandlineNumber || processedContactDestinationNumber.Substring(3, 1).Equals(WorkspaceNoteUpdateComponent._LocalAreaCode))
                                {
                                    MessageBox.Show(String.Format(Messages.MessageCannotSendSMSToNumber, rawDestContactNumber), Messages.MessageBoxTitle);
                                    return;
                                }
                                //else
                                //{
                                //    destContact.Number = processedContactDestinationNumber;
                                //}
                            }
                        }

                        if (_objectProvider.CreateSMSInteraction(contact.Id, RightNowGlobalContext.AccountId.ToString(), processedContactDestinationNumber, /*_openIncidentID*/ 0, SMSDialog.txtMessage.Text.Trim(), model.AlphaNumericHeader, model.OptOutOverride) == false)
                        {
                            MessageBox.Show(Messages.MessageSMSSendFailureInteraction, Messages.MessageBoxTitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.DebugFormat("fnSendSMS Exception :::::: ", ex.Message.ToString());
                        MessageBox.Show(Messages.MessageSMSSendFailure, Messages.MessageBoxTitle);
                    }
                }
                SMSDialog.Close();
            });
            SMSDialog.ShowDialog();
        }

        private bool checkNumberInInternationalFormat(string destContactNumber)
        {
             string pattern = @"[\" + WorkspaceNoteUpdateComponent._InternationalPrefix + "]{1}";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(destContactNumber);
            if (match.Success)
            {
                return true;
            }
            return false;
        }

        private bool checkForLockedNumber(string destContactNumber)
        {
            //Check the number against the Local Country Code
            if (WorkspaceNoteUpdateComponent._LockInternationalCalls && !destContactNumber.Substring(1, 2).Equals(WorkspaceNoteUpdateComponent._LocalCountryCode))
            {
                return true;
            }
            return false;
        }


        private bool DoMagicOnNumber(string destNumber, out string processedNumber, out bool isLandLineNumber)
        {
            bool retVal = true; processedNumber = null; isLandLineNumber = false;
            string cc = util.GetRegionCodeForCountryCode(Convert.ToInt32(WorkspaceNoteUpdateComponent._LocalCountryCode));
            string destContactNumber = destNumber;
            if (destContactNumber.Substring(0, 4).Equals("0011"))
            {
                destContactNumber = String.Format("+{0}", destContactNumber.Substring(5));
            }
            else if (destContactNumber.Substring(0, 1).Equals("+"))
            {
                //Use as it is
                //destContactNumber = String.Format("+{0}", destContactNumber.Substring(5));
            }
            else if (destContactNumber.Length >= 11 && !destContactNumber.Substring(0, 1).Equals("+"))
            {
                //Add a a plus.
                destContactNumber = String.Format("+{0}", destContactNumber);
            }
            else if ((destContactNumber.Length == 10) && (destContactNumber.Substring(0, 2).Equals("04") || destContactNumber.Substring(0, 2).Equals("05")))
            {
                //Add LocalCountryCode & remove leading zero
                destContactNumber = String.Format("{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber.Substring(1));
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Length == 10) && (destContactNumber.Substring(0, 2).Equals("02") || destContactNumber.Substring(0, 2).Equals("03") ||
                destContactNumber.Substring(0, 2).Equals("07") || destContactNumber.Substring(0, 2).Equals("08")))
            {
                isLandLineNumber = true;
                //Add LocalCountryCode & remove leading zero
                destContactNumber = String.Format("{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber.Substring(1));
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Length == 9) && (destContactNumber.Substring(0, 1).Equals("4")))
            {
                //Perhaps just leading zero left off mobile - Add LocalCountryCode and see if it works -> 61 4 12345678
                //Add LocalCountryCode & remove leading zero
                destContactNumber = String.Format("{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber);
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }

            }
            else if ((destContactNumber.Length == 9) && (destContactNumber.Substring(0, 1).Equals("2") || destContactNumber.Substring(0, 1).Equals("3") ||
                destContactNumber.Substring(0, 1).Equals("7") || destContactNumber.Substring(0, 1).Equals("8")))
            {
                isLandLineNumber = true;
                //Probably just leading zero left off - Add LocalCountryCode and see if it works -> 61 123456789
                destContactNumber = String.Format("{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber);
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Length == 9))
            {
                //Same as above but here for simplicity
                destContactNumber = String.Format("{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber);
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else if ((destContactNumber.Substring(0, 1).Equals(WorkspaceNoteUpdateComponent._TrunkAccessCode)))
            {
                destContactNumber = String.Format("{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber.Substring(1));
                try
                {
                    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                }
                catch (Exception ex)
                {
                    retVal = false;
                    Logger.Log.Debug("Exception while converting number having Trunk Acces code to E164", ex);
                }
            }
            else
            {
                if (destContactNumber.Length == 8)
                {
                    isLandLineNumber = true;
                    destContactNumber = String.Format("{0}{1}{2}", WorkspaceNoteUpdateComponent._LocalCountryCode, WorkspaceNoteUpdateComponent._LocalAreaCode, destContactNumber);
                    try
                    {
                        var e164contactDestNumber = util.Parse(destContactNumber, cc);
                        destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        Logger.Log.Debug("Exception while converting LL number to E164", ex);
                    }
                }
                else if (destContactNumber.Length == 10 && WorkspaceNoteUpdateComponent._LocalCountryCode == "1")
                {
                    //10 Digit Number Handling for US.
                    isLandLineNumber = false;
                    destContactNumber = String.Format("+{0}{1}", WorkspaceNoteUpdateComponent._LocalCountryCode, destContactNumber);
                    //try
                    //{
                    //    var e164contactDestNumber = util.Parse(destContactNumber, cc);
                    //    destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                    //}
                    //catch (Exception ex)
                    //{
                    //    retVal = false;
                    //    Logger.Logger.Log.Debug("Exception while converting LL number to E164", ex);
                    //}
                }
                else
                {
                    try
                    {
                        var e164contactDestNumber = util.Parse(destContactNumber, cc);
                        destContactNumber = util.Format(e164contactDestNumber, PhoneNumberFormat.E164);
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        Logger.Log.Debug("Exception while converting any number E164", ex);
                    }
                }
            }
            if (retVal)
            {
                processedNumber = destContactNumber;
            }
            return retVal;
        }
    }
}

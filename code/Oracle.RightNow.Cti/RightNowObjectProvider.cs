// ===========================================================================================
//  Oracle RightNow Connect
//  CTI Sample Code
// ===========================================================================================
//  Copyright © Oracle Corporation.  All rights reserved.
// 
//  Sample code for training only. This sample code is provided "as is" with no warranties 
//  of any kind express or implied. Use of this sample code is pursuant to the applicable
//  non-disclosure agreement and or end user agreement and or partner agreement between
//  you and Oracle Corporation. You acknowledge Oracle Corporation is the exclusive
//  owner of the object code, source code, results, findings, ideas and any works developed
//  in using this sample code.
// ===========================================================================================
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Oracle.RightNow.Cti.ConnectService;
using Oracle.RightNow.Cti.Model;
using RightNow.AddIns.AddInViews;
using System.Collections;
using System.Net.Mail;
using Oracle.RightNow.Cti.AddIn;
using System.Text.RegularExpressions;

namespace Oracle.RightNow.Cti {
    public class RightNowObjectProvider {
        private IGlobalContext _globalContext;

        public RightNowObjectProvider(IGlobalContext globalContext) {
            _globalContext = globalContext;
        }

        public T GetObject<T>(string predicate = null) where T : class, new() {
            var objectType = typeof(T);

            var customObjectAttribute = objectType.GetCustomAttributes(typeof(RightNowCustomObjectAttribute), true).FirstOrDefault() as RightNowCustomObjectAttribute;
            if (customObjectAttribute == null)
                throw new InvalidOperationException("The type provided is not a RightNow custom object type. Please use the RightNowCustomObjectAttribute to associate the proper metadata with the type");

            var query = string.Format("SELECT * from {0}.{1} {2}", customObjectAttribute.PackageName, customObjectAttribute.ObjectName, predicate ?? string.Empty);

            var request = new QueryCSVRequest(getClientInfoHeader(), query, 1, ",", false, true);

            var rightNowChannel = getChannel();
            var result = rightNowChannel.QueryCSV(request);

            return materializeObjects<T>(result, objectType, 1).FirstOrDefault();
        }

        public IList<long> GetObjectIds(string entityName, string predicate) {
            var request = new QueryCSVRequest(getClientInfoHeader(),
                string.Format("SELECT id FROM {0} WHERE {1}", entityName, predicate), 1000, ",", false, true);

            var rightNowChannel = getChannel();

            var result = rightNowChannel.QueryCSV(request);

            var ids = new List<long>();
            if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0) {
                foreach (var row in result.CSVTableSet.CSVTables[0].Rows) {
                    var resultRow = row.Split(',');
                    ids.Add(long.Parse(resultRow[0]));
                }
            }
            return ids;
        }

        public IList<long> GetContactIdFromStudentId(string studentid)
        {
            var request = new QueryCSVRequest(getClientInfoHeader(),
                string.Format("SELECT ID FROM Contact O WHERE O.CustomFields.c.student_id={0}", studentid), 1000, ",", false, true);

            var rightNowChannel = getChannel();

            var result = rightNowChannel.QueryCSV(request);

            var ids = new List<long>();
            if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
            {
                foreach (var row in result.CSVTableSet.CSVTables[0].Rows)
                {
                    var resultRow = row.Split(',');
                    ids.Add(long.Parse(resultRow[0]));
                }
            }
            return ids;
        }

        public string GetPhoneNumberFromContactID(long contactID)
        {
            string phoneNumber = null;
            var request = new QueryCSVRequest(getClientInfoHeader(),
                string.Format("SELECT Phones.* FROM Contact O WHERE O.Id={0}", contactID), 1000, ",", false, true);

            var rightNowChannel = getChannel();

            var result = rightNowChannel.QueryCSV(request);

            var ids = new List<long>();
            if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0 && result.CSVTableSet.CSVTables[0].Rows.Length > 0)
            {
                var resultRow = result.CSVTableSet.CSVTables[0].Rows[1].Split(',');//Mobile Number.
                phoneNumber = resultRow[1]; 
            }
            return phoneNumber;
        }

        public IList<long> GetContactIdFromAccountEmail(string accountEmail)
        {
            var request = new QueryCSVRequest(getClientInfoHeader(),
                string.Format("SELECT ID FROM Contact O WHERE O.Emails.EmailList.Address='{0}'", accountEmail), 1000, ",", false, true);

            var rightNowChannel = getChannel();

            var result = rightNowChannel.QueryCSV(request);

            var ids = new List<long>();
            if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
            {
                foreach (var row in result.CSVTableSet.CSVTables[0].Rows)
                {
                    ids.Add(long.Parse(row));
                }
            }
            return ids;
        }

        public IEnumerable<T> GetObjects<T>() where T : class, new() {
            var objectType = typeof(T);

            var customObjectAttribute = objectType.GetCustomAttributes(typeof(RightNowCustomObjectAttribute), true).FirstOrDefault() as RightNowCustomObjectAttribute;
            if (customObjectAttribute == null)
                throw new InvalidOperationException("The type provided is not a RightNow custom object type. Please use the RightNowCustomObjectAttribute to associate the proper metadata with the type");

            var request = new QueryCSVRequest(getClientInfoHeader(),
                string.Format("SELECT * from {0}.{1}", customObjectAttribute.PackageName, customObjectAttribute.ObjectName), 100, ",", false, true);

            var rightNowChannel = getChannel();
            var result = rightNowChannel.QueryCSV(request);
            
            return materializeObjects<T>(result, objectType);
        }

        public IList<IncidentInfo> GetPendingIncidents(DateTime cutOffTime) {
            var incidents = new List<IncidentInfo>();

            try {
                //  cutOffTime = cutOffTime.AddSeconds(_globalContext.TimeOffset);
                var request = new QueryCSVRequest(getClientInfoHeader(),
                    string.Format(@"SELECT 
	                                                    ID,
	                                                    Channel,
	                                                    Queue.Name,
	                                                    ReferenceNumber,
	                                                    Source.Name SourceName,
                                                        PrimaryContact.Contact ContactId,
                                                        PrimaryContact.Contact.Name ContactName,
                                                        PrimaryContact.ParentContact.Emails.Address Email,
	                                                    Subject,
                                                        UpdatedTime
                                                    FROM Incident
                                                    WHERE UpdatedTime > '{0}'
                                                    AND StatusWithType.StatusType.Id = 1
                                                    AND AssignedTo.Account.Id IS NULL
                                                    AND PrimaryContact.ParentContact.Emails.Address IS NOT NULL", cutOffTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    1000, ",", false, true);

                var rightNowChannel = getChannel();

                var result = rightNowChannel.QueryCSV(request);

                if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0) {
                    foreach (var row in result.CSVTableSet.CSVTables[0].Rows) {
                        var resultRow = row.Split(',');
                        var incident = new IncidentInfo {
                            Id = long.Parse(resultRow[0]),
                            Channel = getRightNowChannel(resultRow[1]),
                            QueueName = resultRow[2],
                            ReferenceNumber = resultRow[3],
                            SourceName = resultRow[4],
                            ContactId = long.Parse(resultRow[5]),
                            ContactName = resultRow[6],
                            ContactEmail = resultRow[7],
                            Subject = resultRow[8],
                            LastUpdate = DateTime.Parse(resultRow[9])
                        };
                        incidents.Add(incident);
                    }
                }
            }
            catch (Exception exc) {
                _globalContext.LogMessage(string.Format("Error polling incidents: {0}", exc));
            }

            return incidents;
        }
  
        private RightNowChannel getRightNowChannel(string channelValue) {
            RightNowChannel channel;
            Enum.TryParse(channelValue, out channel);
            return channel;
        }

        public bool GetCustomerDomainFromBLAccount(out string username, out string password, out string url)
        {
            bool retVal = true;
            username = ""; password = ""; url = "";
            try
            {
                GenericObject go = new GenericObject();

                //Set the object type
                RNObjectType objType = new RNObjectType();
                objType.Namespace = "BLDialogue";
                objType.TypeName = "BLAccount";
                go.ObjectType = objType;

                go.ID = new ID();
                go.ID.id = 1;
                go.ID.idSpecified = true;

                GetProcessingOptions options = new GetProcessingOptions();
                options.FetchAllNames = true;
                GetRequest myGetRequest = new GetRequest(getClientInfoHeader(), new RNObject[] { go }, options);

                GetResponse response = getChannel().Get(myGetRequest);

                RNObject[] results = response.RNObjectsResult;

                // check result and save incident id
                if (results != null && results.Length > 0)
                {
                    foreach (GenericObject result in results)
                    {
                        if (result != null && result.ID != null)
                        {
                            foreach (GenericField field in result.GenericFields)
                            {
                                //if (field.name.ToLower().Equals("customerdomain1"))
                                //{
                                //    customerDomain = field.DataValue.Items[0].ToString() ;
                                 
                                //}
                                if (field.name.ToLower().Equals("blaccountsid"))
                                {
                                    username = field.DataValue.Items[0].ToString();
                                }
                                if (field.name.ToLower().Equals("blaccountpwd"))
                                {
                                    password = field.DataValue.Items[0].ToString();
                                }
                                if (field.name.ToLower().Equals("blaccounturl"))
                                {
                                    url = field.DataValue.Items[0].ToString();
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                Logger.Logger.Log.Debug("Exception during GetCustomerDomain ", ex);
                retVal = false;
            }

            return retVal;
        }
        public StaffAccountInfo GetStaffAccountInformation(int id) {
            var staffAccount = new StaffAccountInfo();
            try
            {
                var request = new QueryCSVRequest(getClientInfoHeader(),
                    string.Format("SELECT DisplayName, CustomFields.BlueleapCTI.FinesseAccount, CustomFields.BlueleapCTI.FinessePasswd, CustomFields.BlueleapCTI.FinesseExtension, CustomFields.BlueleapCTI.FinesseAutoLogin, Phones.*, Emails.* FROM Account WHERE id= {0}", id), 1, ",", false, true);

                var rightNowChannel = getChannel();

                var result = rightNowChannel.QueryCSV(request);

                if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0 && result.CSVTableSet.CSVTables[0].Rows.Length > 0)
                {
                    var resultRow = result.CSVTableSet.CSVTables[0].Rows[0].Split(',');
                    staffAccount.Name = resultRow[0];
                    staffAccount.AcdId = resultRow[1];
                    staffAccount.AcdPassword = resultRow[2];
                    staffAccount.Extension = resultRow[3];
                    staffAccount.FinesseAutoLogin = false;
                    if(!String.IsNullOrEmpty(resultRow[4]))
                    {
                        string value = resultRow[4].ToLower().Trim();
                        if(value.Equals("yes") || value.Equals("1"))
                        {
                            staffAccount.FinesseAutoLogin = true;
                        }
                    }
                    staffAccount.Email = resultRow[9];
                }
            }
            catch (Exception ex) {
                staffAccount = null;
            }
            staffAccount.ContactID = 0;
            return staffAccount;
        }


        public bool UpdateStaffAccountInformation(StaffAccountInfo staffActInfo)
        {
            try
            {
                Logger.Logger.Log.Debug("RightNowObjectProvider: Get User Account Details");
                Account acobj = new Account();
                acobj.ID = new ID() { id = _globalContext.AccountId, idSpecified = true };
                GenericObject genericObject = new GenericObject();
                GenericObject genericObjectFinal = new GenericObject();

                RNObjectType rnobj = new RNObjectType();

                List<GenericField> genericFields = new List<GenericField>();
                List<GenericField> genericField = new List<GenericField>();

                if(staffActInfo.FinesseAutoLogin == false)
                {
                    //Clear out the fields.
                    staffActInfo.AcdId = "";
                    staffActInfo.AcdPassword = "";
                    staffActInfo.Extension = "";
                }

                genericFields.Add(createGenericField("FinesseAccount", ItemsChoiceType.StringValue, staffActInfo.AcdId.ToString()));
                genericFields.Add(createGenericField("FinessePasswd", ItemsChoiceType.StringValue, staffActInfo.AcdPassword));
                genericFields.Add(createGenericField("FinesseExtension", ItemsChoiceType.IntegerValue, Convert.ToInt32(staffActInfo.Extension)));
                genericFields.Add(createGenericField("FinesseAutoLogin", ItemsChoiceType.BooleanValue, staffActInfo.FinesseAutoLogin));

                genericObject.GenericFields = genericFields.ToArray();
                rnobj.TypeName = "AccountCustomFieldsc";
                genericObject.ObjectType = rnobj;

                genericField.Add(createGenericField("BlueleapCTI", ItemsChoiceType.ObjectValue, genericObject));
                genericObjectFinal.GenericFields = genericField.ToArray();
                genericObjectFinal.ObjectType = new RNObjectType() { TypeName = "AccountCustomFields" };
                acobj.CustomFields = genericObjectFinal;

                UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                cpo.SuppressExternalEvents = false;
                cpo.SuppressRules = false;
                UpdateRequest cre = new UpdateRequest(getClientInfoHeader(), new RNObject[] { acobj }, cpo);
                UpdateResponse res = getChannel().Update(cre);                
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("RightNowObjectProvider:", ex);
                return false;
            }
            return true;
        }

        public bool UpdateAccountContactPhone(String phoneNumber, long cID)
        {
            try
            {
                //IList<long> ids = GetContactIdFromAccountEmail(AccountEmail);
                ConnectService.Contact contact = new ConnectService.Contact();
                ID contactID = new ID();
                contactID.id = cID;
                contactID.idSpecified = true;
                contact.ID = contactID;

                Phone[] phone = new Phone[1];
                phone[0] = new Phone();
                phone[0].action = ActionEnum.update;
                phone[0].actionSpecified = true;
                phone[0].Number = phoneNumber;
                NamedID phoneType = new NamedID();
                phoneType.ID = new ID();
                phoneType.ID.id = 1;
                phoneType.ID.idSpecified = true;
                phone[0].PhoneType = phoneType;
                contact.Phones = phone;

                UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                cpo.SuppressExternalEvents = false;
                cpo.SuppressRules = false;
                UpdateRequest cre = new UpdateRequest(getClientInfoHeader(), new RNObject[] { contact }, cpo);
                UpdateResponse res = getChannel().Update(cre);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("UpdateAccountContactPhone: Cannot save phone number", ex);
                return false;
            }
            return true;          
        }
        public bool UpdateStaffAccountInformation(String phoneNumber, bool Remember)
        {
            try
            {
                Logger.Logger.Log.Debug("RightNowObjectProvider: Update Account Phone Number");
                Account acobj = new Account();
                acobj.ID = new ID() { id = _globalContext.AccountId, idSpecified = true };
                GenericObject genericObject = new GenericObject();
                GenericObject genericObjectFinal = new GenericObject();

                RNObjectType rnobj = new RNObjectType();

                List<GenericField> genericFields = new List<GenericField>();
                List<GenericField> genericField = new List<GenericField>();

                if (Remember == false)
                {
                    //Clear out the fields.
                    phoneNumber = "";
                    Phone[] phone = new Phone[1];
                    phone[0] = new Phone();
                    phone[0].action = ActionEnum.remove;
                    phone[0].actionSpecified = true;
                    acobj.Phones = phone;
                } else
                {
                    Phone[] phone = new Phone[1];
                    phone[0] = new Phone();
                    phone[0].action = ActionEnum.update;
                    phone[0].actionSpecified = true;
                    phone[0].Number = phoneNumber;
                    NamedID phoneType = new NamedID();
                    phoneType.ID = new ID();
                    phoneType.ID.id = 0;
                    phoneType.ID.idSpecified = true;
                    phone[0].PhoneType = phoneType;
                    acobj.Phones = phone;
                }

                UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                cpo.SuppressExternalEvents = false;
                cpo.SuppressRules = false;
                UpdateRequest cre = new UpdateRequest(getClientInfoHeader(), new RNObject[] { acobj }, cpo);
                UpdateResponse res = getChannel().Update(cre);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("UpdateStaffAccountInformation: Cannot save phone number", ex);
                return false;
            }
            return true;
        }

        public string GetStudentIdFromContact(string cID)
        {
            try
            {
                Oracle.RightNow.Cti.ConnectService.Contact contact = new Oracle.RightNow.Cti.ConnectService.Contact();
                ID contactId = new ID();
                contactId.id = Convert.ToInt32(cID);
                contactId.idSpecified = true;
                contact.ID = contactId;
                contact.CustomFields = new GenericObject { };// need to pass an empty CustomFields object of type GenericObject to get all custom fields

                GetProcessingOptions gpo = new GetProcessingOptions();
                gpo.FetchAllNames = true;

                GetProcessingOptions options = new GetProcessingOptions();
                options.FetchAllNames = true;
                GetRequest myGetRequest = new GetRequest(getClientInfoHeader(), new RNObject[] { contact }, options);

                GetResponse response = getChannel().Get(myGetRequest);

                RNObject[] results = response.RNObjectsResult;

                foreach (Oracle.RightNow.Cti.ConnectService.Contact obj in results)
                {
                    GenericObject genericObject = obj.CustomFields; //GenericObject will contain a list of GenericField objects where the name of each one will be the package names
                    foreach (GenericField field in genericObject.GenericFields)
                    {
                        foreach (GenericObject field1 in field.DataValue.Items) //GenericField is holding an ObjectValue of type GenericObject which is the wrapper for each of the individual fields under each package
                        {
                            GenericField[] field2 = field1.GenericFields;
                            foreach (GenericField customField in field2)//GenericObject "wrapper" object will contain an array of GenericField objects
                            {
                                if (customField.name.ToLower().Equals("student_id"))
                                {
                                    return customField.DataValue.Items[0].ToString();
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Logger.Log.Error("GetStudentIdFromContact ", ex);
            }

            return "";
        }

        public GenericField createGenericField(string Name, ItemsChoiceType itemsChoiceType, object Value)
        {
            GenericField gf = new GenericField();
            gf.name = Name;
            gf.dataTypeSpecified = true;
            gf.DataValue = new DataValue();
            gf.DataValue.ItemsElementName = new ItemsChoiceType[] { itemsChoiceType };
            gf.DataValue.Items = new object[] { Value };
            return gf;
        }

        public void CreateInteractionLogObject(Hashtable data)
        {
            GenericObject go = new GenericObject();

            //Set the object type
            RNObjectType objType = new RNObjectType();
            objType.Namespace = "BlueLeapCTI";
            objType.TypeName = "InteractionLog";
            go.ObjectType = objType;

            List<GenericField> gfs = new List<GenericField>();

            try
            {
                int contactId = 0;
                int studentId = 0;
                int incidentID = 0;
                NamedID relationid = new NamedID();
                ID incID = new ID();
                try
                {
                    contactId = Convert.ToInt32(data["Contact_id"]);
                }
                catch (Exception) { }

                if (contactId != 0)
                {
                    incID.id = contactId;
                    incID.idSpecified = true;
                    relationid.ID = incID;
                    gfs.Add(createGenericField("Contact_id", ItemsChoiceType.NamedIDValue, relationid));
                }

                try
                {
                    incidentID = Convert.ToInt32(data["IncidentId"]);
                }
                catch (Exception) { }
                if (incidentID != 0)
                {
					incID = new ID();
					relationid = new NamedID();
                    incID.id = incidentID;
                    incID.idSpecified = true;
                    relationid.ID = incID;
                    gfs.Add(createGenericField("Incident_id", ItemsChoiceType.NamedIDValue, relationid));
                }

                try
                {
                    studentId = Convert.ToInt32(data["Student_id"]);
                }
                catch (Exception) { }

                gfs.Add(createGenericField("Student_id", ItemsChoiceType.IntegerValue, studentId));

                if (!String.IsNullOrEmpty(data["Call_id"].ToString()))
                {
                    gfs.Add(createGenericField("Call_id", ItemsChoiceType.StringValue, data["Call_id"]));
                }

                if (!String.IsNullOrEmpty(data["CV1"].ToString()))
                {
                    gfs.Add(createGenericField("CV1", ItemsChoiceType.StringValue, data["CV1"]));
                }

                if (!String.IsNullOrEmpty(data["CV2"].ToString()))
                {
                    gfs.Add(createGenericField("CV2", ItemsChoiceType.StringValue, data["CV2"]));
                }

                if (!String.IsNullOrEmpty(data["CV3"].ToString()))
                {
                    gfs.Add(createGenericField("CV3", ItemsChoiceType.StringValue, data["CV3"]));
                }

                if (!String.IsNullOrEmpty(data["CV4"].ToString()))
                {
                    gfs.Add(createGenericField("CV4", ItemsChoiceType.StringValue, data["CV4"]));
                }

                if (!String.IsNullOrEmpty(data["CV5"].ToString()))
                {
                    gfs.Add(createGenericField("CV5", ItemsChoiceType.StringValue, data["CV5"]));
                }

                if (!String.IsNullOrEmpty(data["CV6"].ToString()))
                {
                    gfs.Add(createGenericField("CV6", ItemsChoiceType.StringValue, data["CV6"]));
                }

                if (!String.IsNullOrEmpty(data["CV7"].ToString()))
                {
                    gfs.Add(createGenericField("CV7", ItemsChoiceType.StringValue, data["CV7"]));
                }

                if (!String.IsNullOrEmpty(data["CV8"].ToString()))
                {
                    gfs.Add(createGenericField("CV8", ItemsChoiceType.StringValue, data["CV8"]));
                }

                if (!String.IsNullOrEmpty(data["CV9"].ToString()))
                {
                    gfs.Add(createGenericField("CV9", ItemsChoiceType.StringValue, data["CV9"]));
                }

                if (!String.IsNullOrEmpty(data["CV10"].ToString()))
                {
                    gfs.Add(createGenericField("CV10", ItemsChoiceType.StringValue, data["CV10"]));
                }

                if (!String.IsNullOrEmpty(data["Start"].ToString()))
                {
                    try
                    {
                        DateTime startTime = Convert.ToDateTime(data["Start"]);
                        gfs.Add(createGenericField("Start", ItemsChoiceType.DateTimeValue, startTime.ToUniversalTime()));
                    }
                    catch (Exception ex) { }
                }
                if (!String.IsNullOrEmpty(data["Finish"].ToString()))
                {
                    try
                    {
                        DateTime finishTime = Convert.ToDateTime(data["Finish"]);
                        gfs.Add(createGenericField("Finish", ItemsChoiceType.DateTimeValue, finishTime.ToUniversalTime()));
                    }
                    catch (Exception ex) { }
                    
                }

                if (!String.IsNullOrEmpty(data["CallStatus"].ToString()))
                {
                    int callStatus = -1; //Complted
                    if ((data["CallStatus"].ToString().ToLower().Equals("failed")) || (data["CallStatus"].ToString().ToLower().Equals("dropped")))
                    {
                        callStatus = 5; //Failed
                    }
                    else if ((data["CallStatus"].ToString().ToLower().Equals("completed")))
                    {
                        callStatus = 1; //Completed
                    } 
                    if (callStatus != -1)
                    {
                        relationid = new NamedID();
                        incID = new ID();
                        incID.id = callStatus;
                        incID.idSpecified = true;
                        relationid.ID = incID;
                        gfs.Add(createGenericField("CallStatus", ItemsChoiceType.NamedIDValue, relationid));
                    }

                }

                if (!String.IsNullOrEmpty(data["Duration"].ToString()))
                {
                    gfs.Add(createGenericField("Duration", ItemsChoiceType.IntegerValue, data["Duration"]));
                }

                if (!String.IsNullOrEmpty(data["IncomingNumber"].ToString()))
                {
                    gfs.Add(createGenericField("IncomingNumber", ItemsChoiceType.StringValue, data["IncomingNumber"]));
                }

                if (!String.IsNullOrEmpty(data["OutgoingNumber"].ToString()))
                {
                    gfs.Add(createGenericField("OutgoingNumber", ItemsChoiceType.StringValue, data["OutgoingNumber"]));
                }

                if (!String.IsNullOrEmpty(data["Agent"].ToString()))
                {
                    gfs.Add(createGenericField("Agent", ItemsChoiceType.StringValue, data["Agent"]));
                }

                relationid = new NamedID();
                incID = new ID();
                incID.id = 1;
                incID.idSpecified = true;
                relationid.ID = incID;
                gfs.Add(createGenericField("CallSource", ItemsChoiceType.NamedIDValue, relationid));

                relationid = new NamedID();
                incID = new ID();
                incID.id = _globalContext.AccountId;
                incID.idSpecified = true;
                relationid.ID = incID;
                gfs.Add(createGenericField("Agent_id", ItemsChoiceType.NamedIDValue, relationid));

            } catch (Exception ex) { }

            go.GenericFields = gfs.ToArray();

            CreateProcessingOptions options = new CreateProcessingOptions();
            options.SuppressExternalEvents = false;
            options.SuppressRules = false;
            CreateRequest createRequest = new CreateRequest(getClientInfoHeader(), new RNObject[] { go }, options);
            CreateResponse createResponcse = this.getChannel().Create(createRequest);

            RNObject[] results = createResponcse.RNObjectsResult;

            // check result and save incident id
            if (results != null && results.Length > 0)
            {
                foreach (GenericObject result in results)
                {
                    if (result != null && result.ID != null)
                    {
                        System.Console.WriteLine("*** Created " + result.GetType().ToString() + " with ID " + result.ID.id);
                    }
                }
            }
        }

        public bool CreateSMSInteraction(string contactID, string agentID, string destNumber, int incidentID, string message, bool alphanumericHeader, bool optOutOverride)
        {
            try
            {

                GenericObject go = new GenericObject();
                //Set the object type
                RNObjectType objType = new RNObjectType();
                objType.Namespace = "BLDialogue";
                objType.TypeName = "BLOutboundMessage";
                go.ObjectType = objType;

                List<GenericField> gfs = new List<GenericField>();

                try
                {
                    int contactId = 0;
                    int agendId = 0;
                    NamedID relationid = new NamedID();
                    ID incID = new ID();

                    try
                    {
                        contactId = Convert.ToInt32(contactID);
                    }
                    catch (Exception) { }

                    if (contactId != 0)
                    {
                        incID.id = contactId;
                        incID.idSpecified = true;
                        relationid.ID = incID;
                        gfs.Add(createGenericField("ContactID", ItemsChoiceType.NamedIDValue, relationid));
                    }

                    try
                    {
                        agendId = Convert.ToInt32(agentID);
                    }
                    catch (Exception) { }

                    if (agendId != 0)
                    {
                        relationid = new NamedID();
                        incID = new ID();
                        incID.id = agendId;
                        incID.idSpecified = true;
                        relationid.ID = incID;
                        gfs.Add(createGenericField("AgentID", ItemsChoiceType.NamedIDValue, relationid));
                    }

                    if (incidentID != 0)
                    {
                        relationid = new NamedID();
                        incID = new ID();
                        incID.id = incidentID;
                        incID.idSpecified = true;
                        relationid.ID = incID;
                        gfs.Add(createGenericField("IncidentID", ItemsChoiceType.NamedIDValue, relationid));
                    }
                 
                    //gfs.Add(createGenericField("BLAccountSID", ItemsChoiceType.StringValue, "Unknown Value")); //1
                    //gfs.Add(createGenericField("CustomerDomain", ItemsChoiceType.StringValue, "Unknown Value")); //2
                    //gfs.Add(createGenericField("SendTime", ItemsChoiceType.DateTimeValue, System.DateTime.Now)); //3

                    gfs.Add(createGenericField("MessageType", ItemsChoiceType.StringValue, "SMS"));
                    gfs.Add(createGenericField("MessageBody", ItemsChoiceType.StringValue, message));
                    //gfs.Add(createGenericField("MessageSuffix", ItemsChoiceType.StringValue, "Reply Help for more info."));
                    //gfs.Add(createGenericField("MessageDirection", ItemsChoiceType.StringValue, "Outbound"));
                    gfs.Add(createGenericField("ContactToNumber", ItemsChoiceType.StringValue, destNumber));
                    gfs.Add(createGenericField("AlphaNumeric", ItemsChoiceType.BooleanValue, alphanumericHeader));
                    gfs.Add(createGenericField("OptOutOverride", ItemsChoiceType.BooleanValue, optOutOverride));
                    //gfs.Add(createGenericField("GroupName", ItemsChoiceType.StringValue, "Unknown Value"));
                    //gfs.Add(createGenericField("ReportingManager", ItemsChoiceType.StringValue, "Unknown Value"));
                    //gfs.Add(createGenericField("ContactCountry", ItemsChoiceType.StringValue, "Unknown Value")); //Can be known.
                    //gfs.Add(createGenericField("ContactState", ItemsChoiceType.StringValue, "Unknown Value")); //Can be known.
                }
                catch (Exception ex) { }

                go.GenericFields = gfs.ToArray();

                CreateProcessingOptions options = new CreateProcessingOptions();
                options.SuppressExternalEvents = false;
                options.SuppressRules = false;
                CreateRequest createRequest = new CreateRequest(getClientInfoHeader(), new RNObject[] { go }, options);
                CreateResponse createResponcse = this.getChannel().Create(createRequest);

                RNObject[] results = createResponcse.RNObjectsResult;

                // check result and save incident id
                if (results != null && results.Length > 0)
                {
                    foreach (GenericObject result in results)
                    {
                        if (result != null && result.ID != null)
                        {
                            Logger.Logger.Log.Debug("*** Created " + result.GetType().ToString() + " with ID " + result.ID.id);
                        }
                    }
                }
            } catch (Exception ex)
            {
                Logger.Logger.Log.Error("Error in Create SMS Interaction ", ex);
                return false;
            }
            return true;
        }

        public bool CreateUpdateSMSCampaign(string campaignName, DateTime runTime, string message, int contactListId, bool optOutOverride, bool alphaNumericHeader, int agentID, bool isUpdate = false, int campaignID = -1, bool isCancel = false)
        {
            try
            {

                GenericObject go = new GenericObject();
                //Set the object type
                RNObjectType objType = new RNObjectType();
                objType.Namespace = "BLDialogue";
                objType.TypeName = "BLCampaigns";
                go.ObjectType = objType;
                if(isUpdate)
                {
                    ID cID = new ID();
                    cID.id = campaignID;
                    cID.idSpecified = true;
                    go.ID = cID;
                }
                List<GenericField> gfs = new List<GenericField>();

                try
                {
                    if (agentID != 0)
                    {
                        NamedID relationid = new NamedID();
                        ID incID = new ID();
                        incID.id = agentID;
                        incID.idSpecified = true;
                        relationid.ID = incID;
                        gfs.Add(createGenericField("AgentID", ItemsChoiceType.NamedIDValue, relationid));
                    }

                    gfs.Add(createGenericField("MessageType", ItemsChoiceType.StringValue, "Message Campaign"));
                    gfs.Add(createGenericField("ContactListID", ItemsChoiceType.IntegerValue, contactListId));
                    gfs.Add(createGenericField("CampaignName", ItemsChoiceType.StringValue, campaignName));
                    gfs.Add(createGenericField("MessageBody", ItemsChoiceType.StringValue, message));
                    gfs.Add(createGenericField("OptOutOverride", ItemsChoiceType.BooleanValue, optOutOverride));
                    //gfs.Add(createGenericField("MergeStudentID", ItemsChoiceType.BooleanValue, mergeFieldsStudentID));
                    //gfs.Add(createGenericField("MergeFirstName", ItemsChoiceType.BooleanValue, mergeFieldsFirstName));
                    gfs.Add(createGenericField("AlphaNumeric", ItemsChoiceType.BooleanValue, alphaNumericHeader));
                    if (!runTime.Equals(DateTime.MinValue))
                    {
                        gfs.Add(createGenericField("RunTime", ItemsChoiceType.DateTimeValue, runTime));
                    }
                }
                catch (Exception ex) { }

               
                RNObject[] results = null;
                if (isUpdate)
                {
                    UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                    cpo.SuppressExternalEvents = false;
                    cpo.SuppressRules = false;
                    UpdateRequest updateRequest = new UpdateRequest(getClientInfoHeader(), new RNObject[] { go }, cpo);
                    gfs.Add(createGenericField("StatusOrExceptionCode", ItemsChoiceType.StringValue, "bls1025"));
                    gfs.Add(createGenericField("StatusOrExceptionDescription", ItemsChoiceType.StringValue, "Campaign has been updated pending changes to server"));
                    gfs.Add(createGenericField("StatusOrExceptionReason", ItemsChoiceType.StringValue, "UpdatePending"));
                    go.GenericFields = gfs.ToArray();
                    UpdateResponse res = getChannel().Update(updateRequest);
                }
                else
                {
                    if (!runTime.Equals(DateTime.MinValue))
                    {
                        gfs.Add(createGenericField("StatusOrExceptionCode", ItemsChoiceType.StringValue, "bls1023"));
                        gfs.Add(createGenericField("StatusOrExceptionDescription", ItemsChoiceType.StringValue, "Campaign is pending to be scheduled for processing by the server"));
                        gfs.Add(createGenericField("StatusOrExceptionReason", ItemsChoiceType.StringValue, "SchedulePending"));
                    }
                    else
                    {
                        gfs.Add(createGenericField("StatusOrExceptionCode", ItemsChoiceType.StringValue, "bls1022"));
                        gfs.Add(createGenericField("StatusOrExceptionDescription", ItemsChoiceType.StringValue, "Campaign is being processed"));
                        gfs.Add(createGenericField("StatusOrExceptionReason", ItemsChoiceType.StringValue, "Executing"));
                    }

                    CreateProcessingOptions options = new CreateProcessingOptions();
                    options.SuppressExternalEvents = false;
                    options.SuppressRules = false;
                    CreateRequest createRequest = new CreateRequest(getClientInfoHeader(), new RNObject[] { go }, options);
                    go.GenericFields = gfs.ToArray();
                    CreateResponse createResponcse = this.getChannel().Create(createRequest);
                    results = createResponcse.RNObjectsResult;
                }

                // check result and save incident id
                if (results != null && results.Length > 0)
                {
                    foreach (GenericObject result in results)
                    {
                        if (result != null && result.ID != null)
                        {
                            Logger.Logger.Log.Debug("*** Created " + result.GetType().ToString() + " with ID " + result.ID.id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("Error in Creating Campaign ", ex);
                return false;
            }
            return true;
        }

        public bool CancelStopCampaign(int campaignID, bool isCancelled = true)
        {
            try
            {

                GenericObject go = new GenericObject();
                //Set the object type
                RNObjectType objType = new RNObjectType();
                objType.Namespace = "BLDialogue";
                objType.TypeName = "BLCampaigns";
                go.ObjectType = objType;
                ID cID = new ID();
                cID.id = campaignID;
                cID.idSpecified = true;
                go.ID = cID;
                List<GenericField> gfs = new List<GenericField>();

                try
                {

                    if (isCancelled)
                    {
                        gfs.Add(createGenericField("StatusOrExceptionCode", ItemsChoiceType.StringValue, "bls1027"));
                        gfs.Add(createGenericField("StatusOrExceptionDescription", ItemsChoiceType.StringValue, "Campaign has been cancelled pending changes to server"));
                        gfs.Add(createGenericField("StatusOrExceptionReason", ItemsChoiceType.StringValue, "CancelPending"));
                    } else
                    {
                        gfs.Add(createGenericField("StatusOrExceptionCode", ItemsChoiceType.StringValue, "bls1030"));
                        gfs.Add(createGenericField("StatusOrExceptionDescription", ItemsChoiceType.StringValue, "Campaign was stopped while executing"));
                        gfs.Add(createGenericField("StatusOrExceptionReason", ItemsChoiceType.StringValue, "Stopped"));
                    }
                }
                catch (Exception ex) { }
                go.GenericFields = gfs.ToArray();

                RNObject[] results = null;
                UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                cpo.SuppressExternalEvents = false;
                cpo.SuppressRules = false;
                UpdateRequest updateRequest = new UpdateRequest(getClientInfoHeader(), new RNObject[] { go }, cpo);
                UpdateResponse res = getChannel().Update(updateRequest);

                // check result and save incident id
                if (results != null && results.Length > 0)
                {
                    foreach (GenericObject result in results)
                    {
                        if (result != null && result.ID != null)
                        {
                            Logger.Logger.Log.Debug("*** Created " + result.GetType().ToString() + " with ID " + result.ID.id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("Error in Creating Campaign ", ex);
                return false;
            }
            return true;
        }

        public bool CreateUpdateSMSTemplate(ref BLMessageTemplate template, bool isUpdate = false)
        {
            try
            {

                GenericObject go = new GenericObject();
                //Set the object type
                RNObjectType objType = new RNObjectType();
                objType.Namespace = "BLDialogue";
                objType.TypeName = "BLMessageTemplate";
                go.ObjectType = objType;
                if (isUpdate)
                {
                    ID cID = new ID();
                    cID.id = template.Id;
                    cID.idSpecified = true;
                    go.ID = cID;
                }
                List<GenericField> gfs = new List<GenericField>();

                try
                {
                    gfs.Add(createGenericField("TemplateName", ItemsChoiceType.StringValue, template.TemplateName));
                    gfs.Add(createGenericField("MessageBody", ItemsChoiceType.StringValue, template.MessageBody));
                    gfs.Add(createGenericField("CreatedByAgentID", ItemsChoiceType.IntegerValue, template.CreatedByAgentID));
                    
                }
                catch (Exception ex) { }


                RNObject[] results = null;
                if (isUpdate)
                {
                    UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                    cpo.SuppressExternalEvents = false;
                    cpo.SuppressRules = false;
                    UpdateRequest updateRequest = new UpdateRequest(getClientInfoHeader(), new RNObject[] { go }, cpo);
                    go.GenericFields = gfs.ToArray();
                    UpdateResponse res = getChannel().Update(updateRequest);
                }
                else
                {
                    CreateProcessingOptions options = new CreateProcessingOptions();
                    options.SuppressExternalEvents = false;
                    options.SuppressRules = false;
                    CreateRequest createRequest = new CreateRequest(getClientInfoHeader(), new RNObject[] { go }, options);
                    go.GenericFields = gfs.ToArray();
                    CreateResponse createResponcse = this.getChannel().Create(createRequest);
                    results = createResponcse.RNObjectsResult;
                }

                // check result and save incident id
                if (results != null && results.Length > 0)
                {
                    foreach (GenericObject result in results)
                    {
                        if (result != null && result.ID != null)
                        {
                            template.Id = (int)result.ID.id;
                            Logger.Logger.Log.Debug("*** Created " + result.GetType().ToString() + " with ID " + result.ID.id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("Error in CreateUpdateSMSTemplate ", ex);
                return false;
            }
            return true;
        }
        public IList<string> GetContactList()
        {
            var request = new QueryCSVRequest(getClientInfoHeader(),
                "SELECT MarketingSettings.ContactLists.NamedIDList.Name FROM Contact", 50000, ",", false, true);

            var rightNowChannel = getChannel();

            var result = rightNowChannel.QueryCSV(request);

            var ids = new List<string>();
            if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
            {
                foreach (var row in result.CSVTableSet.CSVTables[0].Rows)
                {
                    if (!String.IsNullOrEmpty(row))
                    {
                        ids.Add(row);
                    }
                }
            }

            //try
            //{
            //    RNObjectType rnObjType = new RNObjectType();
            //    rnObjType.Namespace = "folders";
            //    rnObjType.TypeName = "folders";

            //    GenericObject genericObj = new GenericObject();
            //    ID id = new ID();
            //    id.id = 35;
            //    genericObj.ID = id;
            //    genericObj.ObjectType = rnObjType;

            //    RNObject[] objectTemplates = new RNObject[] { genericObj };

            //    String Stringroql = "SELECT folders from folders";
            //    QueryObjectsRequest req = new QueryObjectsRequest();
            //    req.ClientInfoHeader = getClientInfoHeader();
            //    req.ObjectTemplates = objectTemplates;
            //    req.PageSize = 10;
            //    req.Query = Stringroql;
            //    QueryObjectsResponse respo = rightNowChannel.QueryObjects(req);
            //} catch(Exception ex)
            //{

            //}
            return ids;
        }

        public bool GetContactListInformation(int ContactListID, out int totalContacts, out string ContactListName)
        {
            bool retVal = true;
            totalContacts = 0;
            ContactListName = "Not Found";
            try
            {
                var request = new QueryCSVRequest(getClientInfoHeader(),
                    String.Format("SELECT MarketingSettings.ContactLists.NamedIDList.Name, count(*) As TotalContacts FROM Contact Where MarketingSettings.ContactLists.NamedIDList.ID={0} LIMIT 1", ContactListID), 1, ",", false, false);

                var rightNowChannel = getChannel();

                var result = rightNowChannel.QueryCSV(request);

                var ids = new List<string>();
                if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
                {
                    totalContacts = Convert.ToInt32(result.CSVTableSet.CSVTables[0].Rows[0].Split(',')[1]);
                    ContactListName = result.CSVTableSet.CSVTables[0].Rows[0].Split(',')[0].ToString();
                }
            } catch (Exception ex)
            {
                Logger.Logger.Log.Debug("Exception during GetContactListInformation ", ex);
            }

            return retVal;
        }

        public DateTime GetCampaignRunTime(int ID)
        {
            DateTime retVal = DateTime.MinValue;
            try
            {
                GenericObject go = new GenericObject();

                //Set the object type
                RNObjectType objType = new RNObjectType();
                objType.Namespace = "BLDialogue";
                objType.TypeName = "BLCampaigns";
                go.ObjectType = objType;

                go.ID = new ID();
                go.ID.id = ID;
                go.ID.idSpecified = true;

                GetProcessingOptions gpo = new GetProcessingOptions();
                gpo.FetchAllNames = false;

                GetRequest request = new GetRequest();
                request.ClientInfoHeader = getClientInfoHeader();
                request.ProcessingOptions = gpo;
                request.RNObjects = new RNObject[] { go };

                GetResponse response = getChannel().Get(request);
                RNObject[] results = response.RNObjectsResult;

                // check result and save incident id
                if (results != null && results.Length > 0)
                {
                    foreach (GenericObject result in results)
                    {
                        if (result != null && result.ID != null)
                        {
                            retVal = Convert.ToDateTime((result.GenericFields[11].DataValue).Items[0]);
                        }
                    }
                }
            }
            catch (FaultException ex)
            {
                Logger.Logger.Log.Debug("GetCampaignMode: " + ex.Code);
                Logger.Logger.Log.Debug("GetCampaignMode: " + ex.Message);
            }
            return retVal;
        }

        public void QueryObjectsSample()
        {
            // String queryString = "SELECT O.Contacts FROM Organization O WHERE O.ID = 1;";
            String queryString = String.Format("SELECT MarketingSettings.ContactLists.NamedIDList.Name, count(*) As TotalContacts FROM Contact Where MarketingSettings.ContactLists.NamedIDList.ID={0} LIMIT 1", 2409);
            ConnectService.Contact contactTemplate = new ConnectService.Contact();
            contactTemplate.MarketingSettings = new ContactMarketingSettings();

            RNObject[] objectTemplates = new RNObject[] { contactTemplate };

            try
            {
                QueryObjectsRequest request = new QueryObjectsRequest();
                request.ClientInfoHeader = getClientInfoHeader();
                request.Query = queryString;
                request.ObjectTemplates = objectTemplates;
                request.PageSize = 10000;
                QueryObjectsResponse response = getChannel().QueryObjects(request);
                QueryResultData[] queryObjects = response.Result;
                RNObject[] rnObjects = queryObjects[0].RNObjectsResult;

                foreach (RNObject obj in rnObjects)
                {
                    ConnectService.Contact contact = (ConnectService.Contact)obj;

                    System.Console.WriteLine("Contact first name: " + contact.Name.First + " Last name: " + contact.Name.Last);

                    Note[] notes = contact.Notes;

                    if (notes != null)
                    {
                        foreach (Note note in notes)
                        {
                            System.Console.WriteLine("Contact note text: " + note.Text);
                        }
                    }
                }

            }
            catch (FaultException ex)
            {
                Console.WriteLine(ex.Code);
                Console.WriteLine(ex.Message);
            }
        }

        public bool GetContactListInformationV1(int ContactListID, out int totalContacts, out string ContactListName, out string createdDate, out string updatedDate)
        {
            bool retVal = true;
            totalContacts = 0;
            ContactListName = "Not Found";
            createdDate = "";
            updatedDate = "";
            try
            {
                AnalyticsReport analyticsReport = new AnalyticsReport();
                ID reportID = new ID();
                int reportId = 109838;
                try
                {
                    reportId = Convert.ToInt32(MediaBarAddIn._BLContactListSearchByID);
                }
                catch (Exception ex) { }

                reportID.id = (long)reportId;
                reportID.idSpecified = true;
                analyticsReport.ID = reportID;
                CSVTableSet thisset = new CSVTableSet();

                AnalyticsReportFilter filter = new AnalyticsReportFilter();
                NamedID operatorId = new NamedID();
                operatorId.ID = new ID();
                operatorId.ID.id = 1;  //  1 is "="
                operatorId.ID.idSpecified = true;
                filter.Operator = operatorId;
                filter.Name = "Name";
                filter.Prompt = "Name";
                filter.Values = new string[1];
                filter.Values[0] = ContactListID.ToString();


                analyticsReport.Filters = new AnalyticsReportFilter[] { filter };
                RunAnalyticsReportRequest request = new RunAnalyticsReportRequest();
                request.ClientInfoHeader = getClientInfoHeader();
                request.AnalyticsReport = analyticsReport;
                request.Delimiter = ",";
                request.ReturnRawResult = false;
                request.DisableMTOM = true;
                request.Start = 0;
                request.Limit = 1;

                RunAnalyticsReportResponse response = getChannel().RunAnalyticsReport(request);

                thisset = response.CSVTableSet;
                CSVTable[] tableResults = thisset.CSVTables;
                String[] data = tableResults[0].Rows;

                foreach (String row in data)
                {
                    string[] contactListData = row.Split(',');
                    ContactListName = contactListData[3].ToString();
                    totalContacts = 0;
                    try {
                        string tContact = contactListData[5];
                        totalContacts = Convert.ToInt32(tContact);
                        createdDate = contactListData[0].Replace("\"", string.Empty).Replace("'", string.Empty);
                        updatedDate = contactListData[6].Replace("\"", string.Empty).Replace("'", string.Empty);
                    } catch (Exception ex)
                    {
                    }
                    Logger.Logger.Log.Debug("Row Data: " + row);
                }
            } catch (Exception ex)
            {
                
                Logger.Logger.Log.Debug("GetContactListInformationV1", ex);
                retVal = false;
            }
            return retVal;
        }

        public bool GetFirstNameLastNameFromContact(int contactID, out string fName, out string lName)
        {
            bool retVal = true; fName = ""; lName = "";
            try
            {
                var request = new QueryCSVRequest(getClientInfoHeader(),
                    String.Format("SELECT Name.First, Name.Last FROM Contact Where ID={0} LIMIT 1", contactID), 1, ",", false, false);

                var rightNowChannel = getChannel();

                var result = rightNowChannel.QueryCSV(request);

                var ids = new List<string>();
                if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
                {
                    fName = result.CSVTableSet.CSVTables[0].Rows[0].Split(',')[0];
                    lName = result.CSVTableSet.CSVTables[0].Rows[0].Split(',')[1];
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Debug("Exception during GetFirstNameLastNameFromContact ", ex);
                retVal = false;
            }
            return retVal;
        }

        public bool GetContactListInformation(string ContactListName, out int totalContacts, out int contactID)
        {
            bool retVal = true;
            totalContacts = 0;
            contactID = 0;
            try
            {
                var request = new QueryCSVRequest(getClientInfoHeader(),
                    String.Format("SELECT MarketingSettings.ContactLists.NamedIDList.Id FROM Contact Where MarketingSettings.ContactLists.NamedIDList.Name='{0}'", ContactListName), 50000, ",", false, true);

                var rightNowChannel = getChannel();

                var result = rightNowChannel.QueryCSV(request);

                var ids = new List<string>();
                if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
                {
                    totalContacts = result.CSVTableSet.CSVTables[0].Rows.Count();
                    contactID = Convert.ToInt32(result.CSVTableSet.CSVTables[0].Rows[0].ToString());
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Debug("Exception during GetContactListInformation ", ex);
            }

            return retVal;
        }

        public bool IsContactHasOptOutFlagSet(string ContactID)
        {
            try
            {
                var request = new QueryCSVRequest(getClientInfoHeader(),
                    String.Format("SELECT CustomFields.BLDialogue.sms_opt_out FROM Contact O WHERE Id = {0}", ContactID), 50000, ",", false, true);

                var rightNowChannel = getChannel();

                var result = rightNowChannel.QueryCSV(request);

                if (result.CSVTableSet != null && result.CSVTableSet.CSVTables.Length > 0)
                {
                    foreach (var row in result.CSVTableSet.CSVTables[0].Rows)
                    {
                        if (!String.IsNullOrEmpty(row))
                        {
                            if (row.Equals("0") || row.Equals("false"))
                                return false;
                            else
                                return true;
                        }
                    }
                }
            } catch (Exception ex)
            {
                Logger.Logger.Log.Debug("Exception in IsContactHasOptOutFlagSet Function ", ex);
            }
            return false;
        }

        public bool ChangeOptOutFlag(long ContactID, bool flag)
        {
            try
            {

                ConnectService.Contact contact = new ConnectService.Contact();
                ID contactID = new ID();
                contactID.id = ContactID;
                contactID.idSpecified = true;
                contact.ID = contactID;

                //Clear out the custom field by setting the DataValue to null (not needed, as DataValue will be null by default, but here to ilustrate this scenario
                //This sample assumes a custom field named con_text with data type of string.
                GenericField customField = new GenericField();
                customField = createGenericField("sms_opt_out", ItemsChoiceType.BooleanValue, flag);
                GenericObject customFieldsc = new GenericObject();
                customFieldsc.GenericFields = new GenericField[] { customField };
                customFieldsc.ObjectType = new RNObjectType() { TypeName = "ContactCustomFieldsc" };

                GenericField customFieldsPackage = new GenericField();
                customFieldsPackage.name = "c";
                customFieldsPackage.dataType = DataTypeEnum.OBJECT;
                customFieldsPackage.dataTypeSpecified = true;
                customFieldsPackage.DataValue = new DataValue();
                customFieldsPackage.DataValue.Items = new[] { customFieldsc };
                customFieldsPackage.DataValue.ItemsElementName = new[] { ItemsChoiceType.ObjectValue };

                contact.CustomFields = new GenericObject
                {
                    GenericFields = new[] { customFieldsPackage },
                    ObjectType = new RNObjectType { TypeName = "ContactCustomFields" }
                };

                RNObject[] contactObjects = new RNObject[] { contact };
                UpdateProcessingOptions cpo = new UpdateProcessingOptions();
                cpo.SuppressExternalEvents = false;
                cpo.SuppressRules = false;
                UpdateRequest cre = new UpdateRequest(getClientInfoHeader(), new RNObject[] { contact }, cpo);
                UpdateResponse res = getChannel().Update(cre);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log.Error("ChangeOptOutFlag: Cannot update opt out flag", ex);
                return false;
            }
            return true;
        }

        public List<string> GetValuesForNamedIDSample(string fieldName, string parent)
        {
            List<string> namedIDvalues = new List<string>();
            try
            {
                //Invoke the GetValuesForNamedID operation, supplying the appropriate string value
                GetValuesForNamedIDRequest request = new GetValuesForNamedIDRequest();
                request.ClientInfoHeader = getClientInfoHeader();
                request.FieldName = fieldName;
                GetValuesForNamedIDResponse response = getChannel().GetValuesForNamedID(request);
                NamedID[] valuesForNamedID = response.Entry;
                //Display the Name and Id properties for each entry
                foreach (NamedID namedID in valuesForNamedID)
                {
                    string attributeName = namedID.Name;
                    if (!string.IsNullOrEmpty(parent))
                    {
                        attributeName = String.Format("{0}.{1}", parent, namedID.Name);
                    }

                    namedIDvalues.Add(attributeName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return namedIDvalues;
        }

        public Dictionary<string,string> GetCustomObjects(long contactID)
        {
            Dictionary<string, string> contactFields = new Dictionary<string, string>();
            try
            {
                ConnectService.Contact contact = new ConnectService.Contact();
                ID contactId = new ID();
                //contactId.id = 926756;
                contactId.id = contactID;
                contactId.idSpecified = true;
                contact.ID = contactId;
                contact.CustomFields = new GenericObject { };// need to pass an empty CustomFields object of type GenericObject to get all custom fields

                GetProcessingOptions gpo = new GetProcessingOptions();
                gpo.FetchAllNames = true;

                GetRequest request = new GetRequest();
                request.ClientInfoHeader = getClientInfoHeader();
                request.ProcessingOptions = gpo;
                request.RNObjects = new RNObject[] { contact };

                GetResponse response = getChannel().Get(request);
                RNObject[] results = response.RNObjectsResult;

                foreach (ConnectService.Contact obj in results)
                {
                    GenericObject genericObject = obj.CustomFields; //GenericObject will contain a list of GenericField objects where the name of each one will be the package names
                    foreach (GenericField field in genericObject.GenericFields)
                    {
                        foreach (GenericObject field1 in field.DataValue.Items) //GenericField is holding an ObjectValue of type GenericObject which is the wrapper for each of the individual fields under each package
                        {
                            GenericField[] field2 = field1.GenericFields;
                            foreach (GenericField customField in field2)//GenericObject "wrapper" object will contain an array of GenericField objects
                            {
                                contactFields.Add(String.Format("{0} ({1})",customField.name, String.Format("CustomFields.{0}.{1}", field.name, customField.name)), String.Format("CustomFields.{0}.{1}", field.name,customField.name));
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                Logger.Logger.Log.Debug("Error during GetCustomObjects", ex);
            }
            return contactFields;
        }
        public Dictionary<string, string> GetContactFields(string className, string name = "")
        {
            //Build an array of strings for the classes we want the meta-data for
            String[] classes = new String[] { className };
            Dictionary<string, string> contactFields = new Dictionary<string, string>();
            //Invoke the GetMetaDataForClass operation
            try
            {

                RNObjectType[] objTypes = new RNObjectType[] { };

                GetMetaDataForClassRequest request = new GetMetaDataForClassRequest(); ;
                request.ClientInfoHeader = getClientInfoHeader();
                request.ClassName = classes;
                request.QualifiedClassName = objTypes;

                GetMetaDataForClassResponse response = getChannel().GetMetaDataForClass(request);
                MetaDataClass[] metaDataForClass = response.MetaDataClass;

                //Process the results, for demonstration purposes we only list a few of the properties
                foreach (MetaDataClass metaDataClass in metaDataForClass)
                {
                    foreach (MetaDataAttribute attribute in metaDataClass.Attributes)
                    {
                        if (attribute.DataType == DataTypeEnum.OBJECT)
                        {
                            if (attribute.UsageOnGet != MetaDataUsageEnum.NOT_ALLOWED && !attribute.Label.Equals("CustomFields") && !attribute.Label.Equals("action"))
                            {
                                Dictionary<string, string> res = GetContactFields(attribute.DataTypeName, attribute.Name);
                                try
                                {
                                    contactFields = contactFields.Concat(res).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex)
                                {
                                    int a = 100;
                                }
                            }
                        }
                        else if (attribute.DataType == DataTypeEnum.OBJECT_LIST)
                        {
                            if (attribute.UsageOnGet != MetaDataUsageEnum.NOT_ALLOWED && !attribute.Label.Equals("CustomFields") && !attribute.Label.Equals("action"))
                            {
                                Dictionary<string, string> res = GetContactFields(attribute.DataTypeName, name);
                                try
                                {
                                    contactFields = contactFields.Concat(res).ToDictionary(x => x.Key, x => x.Value);
                                }
                                catch (Exception ex)
                                {
                                    int a = 100;
                                }
                            }
                        }
                        else if (attribute.DataType == DataTypeEnum.STRING || attribute.DataType == DataTypeEnum.INTEGER) // I don't know what to do with these types yet.
                        {
                            string attributeName = attribute.Name;
                            string key = attribute.Name;
                            if (!string.IsNullOrEmpty(name))
                            {
                                attributeName = String.Format("{0}.{1}", name, attribute.Name);
                                if(!string.IsNullOrEmpty(attributeName))
                                {
                                    key = String.Format("{0} ({1})", attribute.Name, attributeName);
                                }
                            }
                            
                            contactFields.Add(key, attributeName);
                        } 
						/*else if (attribute.DataType == DataTypeEnum.NAMED_ID)
                        {
                            contactFields.AddRange(GetValuesForNamedIDSample(attribute.DataTypeName, attribute.Name));
                        }
						*/
                    }
                }
            }
            catch (FaultException ex)
            {
                Console.WriteLine(ex.Code);
                Console.WriteLine(ex.Message);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine(ex.StatusCode);
                Console.WriteLine(ex.Message);
            }
            //contactFields.AddRange(GetContactCustomFields());
            return contactFields;
        }

        public Dictionary<string, string> GetContactCustomFields()
        {
            //Build an array of strings for the classes we want the meta-data for
            String[] classes = new String[] { "ContactCustomFieldsc" };
            Dictionary<string, string> contactFields = new Dictionary<string, string>();
            //Invoke the GetMetaDataForClass operation
            try
            {

                RNObjectType[] objTypes = new RNObjectType[] { };
                string[] metaDataLink = new string[] { "ContactCustomFieldsc" };
                GetMetaDataForClassRequest request = new GetMetaDataForClassRequest(); ;
                request.ClientInfoHeader = getClientInfoHeader();
                request.ClassName = classes;
                request.QualifiedClassName = objTypes;
                request.MetaDataLink = metaDataLink;
                GetMetaDataForClassResponse response = getChannel().GetMetaDataForClass(request);
                MetaDataClass[] metaDataForClass = response.MetaDataClass;

                //Process the results, for demonstration purposes we only list a few of the properties
                foreach (MetaDataClass metaDataClass in metaDataForClass)
                {
                    foreach (MetaDataAttribute attribute in metaDataClass.Attributes)
                    {
                        if (attribute.DataType == DataTypeEnum.STRING || attribute.DataType == DataTypeEnum.INTEGER) // I don't know what to do with these types yet.
                        {
                            contactFields.Add(String.Format("{0} ({1})",attribute.Name, String.Format("CustomFields.c.{0}", attribute.Name)), String.Format("CustomFields.c.{0}",attribute.Name));
                        }
                    }
                }
            }
            catch (FaultException ex)
            {
                Console.WriteLine(ex.Code);
                Console.WriteLine(ex.Message);
            }
            catch (SmtpException ex)
            {
                Console.WriteLine(ex.StatusCode);
                Console.WriteLine(ex.Message);
            }
            return contactFields;
        }

        private static string[] SplitCSV(string input)
        {
            try
            {
                Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
                List<string> list = new List<string>();
                string curr = null;
                foreach (Match match in csvSplit.Matches(input))
                {
                    curr = match.Value;
                    if (0 == curr.Length)
                    {
                        list.Add("");
                    }

                    list.Add(curr.TrimStart(','));
                }

                return list.ToArray<string>();
            } catch(Exception ex)
            {
                return input.Split(',');
            }
        }


        public List<SMSCampaignModel> GetCampaigns(int rID)
        {
            List<SMSCampaignModel> listCampaigns = new List<SMSCampaignModel>();

            try
            {
                AnalyticsReport analyticsReport = new AnalyticsReport();
                ID reportID = new ID();
                int reportId = rID;
               

                reportID.id = (long)reportId;
                reportID.idSpecified = true;
                analyticsReport.ID = reportID;
                CSVTableSet thisset = new CSVTableSet();
                RunAnalyticsReportRequest request = new RunAnalyticsReportRequest();
                request.ClientInfoHeader = getClientInfoHeader();
                request.AnalyticsReport = analyticsReport;
                //request.Delimiter = ",";
                request.ReturnRawResult = false;
                request.DisableMTOM = true;
                request.Start = 0;
                request.Limit = 10000;
                RunAnalyticsReportResponse response = getChannel().RunAnalyticsReport(request);

                thisset = response.CSVTableSet;
                CSVTable[] tableResults = thisset.CSVTables;
                String[] data = tableResults[0].Rows;

                foreach (String row in data)
                {
                    string[] campaignData = SplitCSV(row);// row.Split(',');
                    SMSCampaignModel campaign = new SMSCampaignModel();
                    try
                    {
                        campaign.ID = Convert.ToInt32(campaignData[0]);                        
                        campaign.Status = campaignData[1];
                        campaign.CampaignName = campaignData[2].Replace('"', ' ').Trim();
                        campaign.MessageBody = campaignData[5].Replace('"', ' ').Trim();
                        campaign.Alpha = campaignData[6];
                        campaign.Override = campaignData[7];
                        //campaign.Total = Convert.ToInt32(campaignData[7]);
                        //campaign.OptOut = Convert.ToInt32(campaignData[8]);
                        campaign.AgentID = Convert.ToInt32(campaignData[9]);
                        campaign.ListID = Convert.ToInt32(campaignData[8]);
                        campaign.AgentName = campaignData[10].Replace('"', ' ').Trim();

                        if (!string.IsNullOrEmpty(campaignData[3]))
                            campaign.DateCreated = Convert.ToDateTime(campaignData[3].Replace("\"", string.Empty).Replace("'", string.Empty));
                        if (!string.IsNullOrEmpty(campaignData[4]))
                        {
                            campaign.RunTime = Convert.ToDateTime(campaignData[4].Replace("\"", string.Empty).Replace("'", string.Empty));
                            //if(campaign.RunTime > DateTime.Now)
                            //{
                            //    campaign.isQueued = "Queued";
                            //} else
                            //{
                            //    campaign.isQueued = "Sent";
                            //}
                        }
                        //if (!string.IsNullOrEmpty(campaignData[11]))
                        //    campaign.StartTime = Convert.ToDateTime(campaignData[11].Replace("\"", string.Empty).Replace("'", string.Empty));
                        //if (!string.IsNullOrEmpty(campaignData[12]))
                        //    campaign.FinishTime = Convert.ToDateTime(campaignData[12].Replace("\"", string.Empty).Replace("'", string.Empty));
                    }
                    catch (Exception ex)
                    {
                        Logger.Logger.Log.Debug("Exception GetCampaigns ", ex);
                    }
                    listCampaigns.Add(campaign);
                    Logger.Logger.Log.Debug("Row Data: " + row);
                }
            }
            catch (Exception ex)
            {

                Logger.Logger.Log.Debug("GetCampaigns", ex);
            }
            return listCampaigns;
        }

        public bool CreateIncident(long contactID, out long incidentID)
        {
            bool retVal = true; incidentID = -1;
            Incident newIncident = new Incident();
            newIncident.Subject = String.Format("Incident Created based on the incoming CISCO CTI Call with Contact ID {0}", contactID);

            //Create an IncidentContact to add as the primary contact on the new Incident
            IncidentContact incContact = new IncidentContact();
            NamedID contactNamedID = new NamedID();
            contactNamedID.ID = new ID();
            contactNamedID.ID.id = contactID;
            contactNamedID.ID.idSpecified = true;
            incContact.Contact = contactNamedID;
            newIncident.PrimaryContact = incContact;

            //Build the RNObject[]
            RNObject[] newObjects = new RNObject[] { newIncident };

            //Set the processing options
            CreateProcessingOptions options = new CreateProcessingOptions();
            options.SuppressExternalEvents = false;
            options.SuppressRules = false;

            //Invoke the Create Operation
            try
            {
                CreateRequest createRequest = new CreateRequest(getClientInfoHeader(),  newObjects, options);
                CreateResponse createResponcse = this.getChannel().Create(createRequest);

                RNObject[] results = createResponcse.RNObjectsResult;
                incidentID = results[0].ID.id;
                Logger.Logger.Log.Debug("New Incident with ID: " + results[0].ID.id + " created.");
            }
            catch (FaultException ex)
            {
                retVal = false;
                Logger.Logger.Log.Debug("Exception during CreateIncident", ex);
            }
            return retVal;
        }

        private RightNowSyncPortChannel getChannel() {
            Binding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            ((BasicHttpBinding)binding).Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            ((BasicHttpBinding)binding).MaxBufferSize = 1024 * 1024 * 1024;
            ((BasicHttpBinding)binding).MaxReceivedMessageSize = 1024 * 1024 * 1024;
            BindingElementCollection elements = binding.CreateBindingElements();
            elements.Find<SecurityBindingElement>().IncludeTimestamp = false;

            binding = new CustomBinding(elements); 

            var channelFactory = new ChannelFactory<RightNowSyncPortChannel>(binding, new EndpointAddress(_globalContext.GetInterfaceServiceUrl(ConnectServiceType.Soap)));

            _globalContext.PrepareConnectSession(channelFactory);

            var rightNowChannel = channelFactory.CreateChannel();
            return rightNowChannel;
        }
  
        private ClientInfoHeader getClientInfoHeader() {
            return new ClientInfoHeader { AppID = "Oracle RightNow CTI" };
        }

        private IEnumerable<T> materializeObjects<T>(QueryCSVResponse result, Type objectType, int objectCount = 100) where T : class, new() {
            var entities = new List<T>();
            if (result.CSVTableSet != null && result.CSVTableSet.CSVTables != null && result.CSVTableSet.CSVTables.Length > 0) {
                var table = result.CSVTableSet.CSVTables[0];
                var columns = new List<string>(table.Columns.Split(','));
                var mapping = new Dictionary<int, PropertyInfo>();

                var properties = objectType.GetProperties();
                foreach (var property in properties) {
                    var attribute = property.GetCustomAttributes(typeof(RightNowCustomObjectFieldAttribute), true).FirstOrDefault() as RightNowCustomObjectFieldAttribute;
                    if (attribute != null) {
                        int position = columns.IndexOf(attribute.Name);
                        if (position > -1) {
                            mapping.Add(position, property);
                        }
                    }
                }

                foreach (var row in table.Rows) {
                    var values = row.Split(',');
                    var entity = new T();
                    foreach (var entry in mapping.Keys) {
                        var propertyInfo = mapping[entry];
                        propertyInfo.SetValue(entity, parse(values[entry], propertyInfo.PropertyType), null);
                    }
                      
                    entities.Add(entity);
                }
            }

            return entities;
        }

        private object parse(string value, Type propertyType) {
            object result = null;
            
            if (propertyType == typeof(String)) {
                result = value;
            }
            else if (propertyType == typeof(Int32)) {
                result = int.Parse(value);
            }
            else if (propertyType == typeof(Boolean)) {
                result = string.Compare(value, "1") == 0;
            }
            else if (propertyType.IsEnum) {
                if (Enum.IsDefined(propertyType, int.Parse(value))) {
                    result = Enum.Parse(propertyType, value);
                }
            }

            return result;
        }
    }
}
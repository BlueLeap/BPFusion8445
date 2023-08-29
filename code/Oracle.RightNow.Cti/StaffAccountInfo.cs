namespace Oracle.RightNow.Cti {
    public class StaffAccountInfo {
        public string Name { get; set; }
        public string AcdId { get; set; }
        public string AcdPassword { get; set; }
        public string Extension { get; set; }
        public string FinesseIP { get; set; }
        public string FinessePort { get; set; }
        public bool FinesseAutoLogin { get; set; }
  
        public string Phone { get; set; }

        public string Email { get; set; }

        public long ContactID { get; set; }
    }
}
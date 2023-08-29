using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oracle.RightNow.Cti {
    public class IncidentInfo {
        public long Id { get; set; }
        public RightNowChannel Channel { get; set; }
        public string QueueName { get; set; }
        public string ReferenceNumber { get; set; }
        public string SourceName { get; set; }
        public long ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string Subject { get; set; }
        public DateTime LastUpdate { get; set; }
    }

    public enum RightNowChannel {
        Email = 1,
        Web = 6
    }
}

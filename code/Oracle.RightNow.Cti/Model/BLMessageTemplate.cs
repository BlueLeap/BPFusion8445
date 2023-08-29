using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oracle.RightNow.Cti.Model
{
    [RightNowCustomObjectAttribute("BLDialogue", "BLMessageTemplate")]
    public class BLMessageTemplate
    {
        public BLMessageTemplate()
        {
        }

        public override string ToString()
        {
            return TemplateName ?? string.Empty;
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null || obj.GetType() != typeof(AgentState))
                return false;

            var other = (BLMessageTemplate)obj;
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }
        #region Property(ies)
        [RightNowCustomObjectField("ID")]
        public int Id { get; set; }

        [RightNowCustomObjectField("CreatedByAgentID")]
        public int CreatedByAgentID { get; set; }

        [RightNowCustomObjectField("MessageBody")]
        public string MessageBody { get; set; }

        [RightNowCustomObjectField("TemplateName")]
        public string TemplateName { get; set; }

        #endregion Property(ies)
    }
}

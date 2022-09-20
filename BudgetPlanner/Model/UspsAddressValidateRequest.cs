using System.Xml.Serialization;

namespace BudgetPlanner.Models
{
    [XmlRoot("AddressValidateRequest")]
    public class UspsAddressValidateRequest
    {
        [XmlAttribute("USERID")]
        public string UserId { get; set; }

        public int Revision { get; set; }
        public UspsAddress Address { get; set; }

        [XmlElement("DPVConfirmation")]
        public string DpvConfirmation { get; set; }
    }
}
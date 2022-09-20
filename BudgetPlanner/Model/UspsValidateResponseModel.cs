using System.Xml.Serialization;

namespace BudgetPlanner.Models
{
    [XmlRoot("AddressValidateResponse")]
    public class UspsAddressValidateResponse
    {
        public UspsAddress Address { get; set; }
    }
}
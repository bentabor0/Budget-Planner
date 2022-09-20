using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using BudgetPlanner.Models;

namespace BudgetPlanner.Services
{
    public class UspsService
    {
        private readonly string _uspsAddressVerificationUrl;

        private readonly string _apiToken;

        public UspsService(string uspsAddressVerificationUrl, string apiToken)
        {
            _uspsAddressVerificationUrl = uspsAddressVerificationUrl;
            _apiToken = apiToken;
        }

        private string GenerateRequestXml(UspsAddressValidateRequest uspsAddressValidationRequest)
        {
            string resultXml;

            // XML serializer will only write to a stream. Make a stream that points at memory instead of a file / network socket.
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serial = new XmlSerializer(typeof(UspsAddressValidateRequest));
                serial.Serialize(stream, uspsAddressValidationRequest);

                // Convert stream contents to string.
                resultXml = Encoding.UTF8.GetString(stream.ToArray());
            }

            return resultXml;
        }

        private UspsAddressValidateResponse ParseResponseXml(string response)
        {
            UspsAddressValidateResponse resultObj;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(response)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UspsAddressValidateResponse));
                resultObj = (UspsAddressValidateResponse)serializer.Deserialize(stream);
            }

            return resultObj;
        }

        public async Task<bool> ValidateAddress(
            string address,
            string address2,
            string city,
            string state,
            string zip)
        {
            // Make validation request.
            UspsAddressValidateRequest uspsAddressValidateRequest = new UspsAddressValidateRequest();
            uspsAddressValidateRequest.UserId = _apiToken;
            uspsAddressValidateRequest.Revision = 1;
            uspsAddressValidateRequest.Address = new UspsAddress()
            {
                Address1 = address ?? "",
                Address2 = address2 ?? "",
                City = city ?? "",
                State = state ?? "",
                Zip5 = zip ?? "",
                Zip4 = ""
            };

            string xml = GenerateRequestXml(uspsAddressValidateRequest);
            string path = _uspsAddressVerificationUrl + HttpUtility.UrlEncode(xml);

            // Make request to API.
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(path);

            // Throw exepction if not valid status.
            response.EnsureSuccessStatusCode();

            string responseData = await response.Content.ReadAsStringAsync();

            // Parse response.
            UspsAddressValidateResponse apiResponse = ParseResponseXml(responseData);

            // Check response.
            return apiResponse.Address?.DpvConfirmation?.ToLower() == "y";
        }
    }
}
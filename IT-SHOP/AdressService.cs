using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShopApp
{
    public class AddressSuggestion
    {
        public string value { get; set; }
        public string unrestricted_value { get; set; }
    }

    public class DaDataResponse
    {
        public List<AddressSuggestion> suggestions { get; set; }
    }

    public class AddressService
    {
        private const string ApiUrl = "https://suggestions.dadata.ru/suggestions/api/4_1/rs/suggest/address";
        private const string ApiKey = "0b373c8687333704ebc51e3f30bdb281f107b5bb"; 

        public async Task<List<AddressSuggestion>> GetAddressSuggestionsAsync(string query)
        {
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers[HttpRequestHeader.Authorization] = "Token " + ApiKey;
                var requestBody = JsonConvert.SerializeObject(new { query = query, count = 10 });
                string jsonResponse = await client.UploadStringTaskAsync(ApiUrl, requestBody);
                var result = JsonConvert.DeserializeObject<DaDataResponse>(jsonResponse);
                return result?.suggestions ?? new List<AddressSuggestion>();
            }
        }
    }
}
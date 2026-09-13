using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
//using System.Text.Json.Serialization;

namespace BankingApp
{
    public class SendTransactions
    {
        private string baseurl = Environment.GetEnvironmentVariable("DB_API_URL_CLDFLR") ?? string.Empty; // API URL FOR DB
        public SendTransactions(){} // default constructor

        public async Task<bool> RecordTransaction(
            string FirstName, string LastName, int CustomerId, 
            object new_payload, string path)
        {
            string url = baseurl + path;

            var accessId = Environment.GetEnvironmentVariable("DB_ACCESS_KEYCLDFLF_TOKEN");
            if (string.IsNullOrWhiteSpace(accessId))
                throw new InvalidOperationException("ACCESS_ID environment variable is not set.");

            var payload = new_payload;

            var json = System.Text.Json.JsonSerializer.Serialize(
                payload,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = null
                }
            );

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("ACCESS_ID", accessId);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);

            if(!response.IsSuccessStatusCode)
                return false; 

            var responseBody = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseBody);

            if(doc.RootElement.TryGetProperty("success", out var statusElement))
                return statusElement.GetBoolean();

            return false;
            
        }

        public async Task<int> GetCustomerIdAsync(string path)
        {
            using var client = new HttpClient();

            // set URL
            string url = baseurl + path;
            var result = await client.GetFromJsonAsync<CustomerResponse>(url);    
            return result.CustomerId;
        }

        public record CustomerResponse(int CustomerId);
    }
}
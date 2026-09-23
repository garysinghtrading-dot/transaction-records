using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BankingApp
{
    public class CustomerSearchPayload
    {
        public string UserName {get; set; } = string.Empty;
    }

    public class Transaction
    {
        [JsonPropertyName("TransactionId")]
        public int TransactionId { get; set; }

        [JsonPropertyName("TransactionType")]
        public string TransactionType { get; set; } = string.Empty;

        [JsonPropertyName("Amount")]
        public decimal Amount { get; set; }
    }

    public class CustomerResponse
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("balance")]
        public decimal? Balance { get; set; }

        [JsonPropertyName("CustomerId")]
        public int? CustomerId { get; set; }

        [JsonPropertyName("Transactions")]
        public List<Transaction> Transactions { get; set; } = new();
    }

    public class GetCustomerData
    {
        private string baseurl = Environment.GetEnvironmentVariable("DB_API_URL_CLDFLR") ?? string.Empty; // API URL FOR DB
        private static readonly HttpClient _httpClient = new HttpClient();
        public GetCustomerData(){} // default constructor

        public async Task<CustomerResponse?> FetchData(
            object newPayload, string path
        )
        {
            string apiUrl = baseurl + path; // URL for API Request
            
            string jsonPayload = JsonSerializer.Serialize(newPayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string responseJson = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Ignores camelCase vs PascalCase differences
            };

            return JsonSerializer.Deserialize<CustomerResponse>(responseJson, options);
            
        }
    }
}
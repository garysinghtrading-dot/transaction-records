using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BankingApp
{
    public class CognitoConfig
    {
        public string UserPoolId { get; set; }
        public string AppClientId { get; set; }
        public string AppClientSecret { get; set; }
    }
    
    public class Verify
    {
        private static string baseurl = Environment.GetEnvironmentVariable("DB_API_URL_CLDFLR") ?? string.Empty; // API URL FOR DB
        public Verify() {} // Default Constructor

        // Helper method to compute Cognito Secret Hash if a client secret exists
        private static string CalculateSecretHash(string clientSecret, string clientId, string username)
        {
            var message = Encoding.UTF8.GetBytes(username + clientId);
            var secretBytes = Encoding.UTF8.GetBytes(clientSecret);
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hashBytes = hmac.ComputeHash(message);
                return Convert.ToBase64String(hashBytes);
            }
        }
        
        private static string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);
        
            // Derive a 256-bit subkey using PBKDF2 with 100,000 iterations
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256
            );
        
            byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash
        
            // Combine salt + hash into one array
            byte[] combined = new byte[salt.Length + hash.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, combined, salt.Length, hash.Length);
        
            // Return Base64(salt + hash)
            return Convert.ToBase64String(combined);
        }
        
        private static bool VerifyPassword(string password, string storedHash)
        {
            byte[] combined = Convert.FromBase64String(storedHash);
        
            // Extract salt (first 16 bytes)
            byte[] salt = new byte[16];
            Buffer.BlockCopy(combined, 0, salt, 0, 16);
        
            // Extract hash (next 32 bytes)
            byte[] storedSubkey = new byte[32];
            Buffer.BlockCopy(combined, 16, storedSubkey, 0, 32);
        
            // Recompute hash with same salt + iterations
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256
            );
        
            byte[] computedSubkey = pbkdf2.GetBytes(32);
        
            // Compare byte-by-byte
            return CryptographicOperations.FixedTimeEquals(storedSubkey, computedSubkey);
        }

        
        public string GetPassWordHash(string password)
        {
            return HashPassword(password);
        }        
        
        private static string GenerateAccessToken(int size = 32)
        {
            byte[] tokenBytes = RandomNumberGenerator.GetBytes(size);
            return Convert.ToBase64String(tokenBytes);
        }

        public void WriteData(Dictionary<string, object> RespObj)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(RespObj);
            Console.WriteLine(json);
        }
        
        /*
            * Method to verify user from localDB
        */
        public Dictionary<string, object> VerifyLocally(string username, string password)
        {
            var responseObj = new Dictionary<string, object>();
            responseObj["authenticated"] = false;
        
            using var conn = new SqliteConnection("Data Source=customers.db");
            conn.Open();
        
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT Password
                FROM Customers
                WHERE UserName = $uname;
            ";
            cmd.Parameters.AddWithValue("$uname", username);
        
            var storedHash = cmd.ExecuteScalar();
        
            if (storedHash == null || storedHash == DBNull.Value)
            {
                responseObj["status"] = "Invalid Username";
                // write data (Authentication Failed);
                return responseObj;
            }
            
            // cconvert DB value to string
            string StoredHashString = storedHash.ToString();

            
            // verify Password
            bool PasswordMatch = VerifyPassword(password, StoredHashString);
            
            if(!PasswordMatch)
            {
                responseObj["status"] = "Invalid Password";
                return responseObj;
            }
            
            // At this point the user has been able to be verified
            responseObj["authenticated"] = true;
            responseObj["access_token"] = GenerateAccessToken();
            responseObj["status"] = "Username and password match";
            return responseObj;  
        }

        /*
            * Method to make HTTP request to AWS Lambda function to retrieve a password
            * IMPORTANT:
                * Password is hashed, it is not returning a raw user password
            * Process:
                * Make HTTP request with JSON data payload
                * Verify the server responded
                * Veryify Password
                * Return Response
                * Response that is returned includes:
                    * Customers Password (Hashed)
                    * CustomerId
        */
        public static async Task<Dictionary<string, object>> GetHashedPassword(string passwordInput, string path = "get-password")
        {
            var response_object = new Dictionary<string, object>();
            response_object["msg"] = "User not found";
            response_object["Password"] = string.Empty;

            string url_ = baseurl + path;
            try
            {
                using HttpClient client = new HttpClient();

                var payload = new { UserName = passwordInput };
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };

                string jsonPayload = JsonSerializer.Serialize(payload, options);
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url_, content);

                if (!response.IsSuccessStatusCode)
                {
                    response_object["msg"] = "Sorry we encountered an error, please try again later";
                    return response_object;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JsonNode? node = JsonNode.Parse(jsonResponse);

                bool isStatusTrue = node?["success"]?.GetValue<bool>() ?? false;

                if (isStatusTrue)
                {
                    string? hashedPassword = node?["Password"]?.GetValue<string>();
                    int customerId = node?["CustomerId"]?.GetValue<int>() ?? (0-100);

                    if (!string.IsNullOrEmpty(hashedPassword))
                    {
                        response_object["CustomerId"] = customerId;
                        response_object["Password"] = hashedPassword;
                        response_object["msg"] = "Customer Found";
                        return response_object; // Properly return the populated dictionary
                    }
                }
                return response_object;
            }
            catch
            {
                response_object["msg"] = "User not found";
                return response_object;
            }
        }

        /*
            * Method to verify customer from AWS DB
        */
        public Dictionary<string, object> VerifyAWS(string username, string password)
        {
            Dictionary<string, object> responseObj = GetHashedPassword(username).GetAwaiter().GetResult();
                
                // Safely retrieve "msg" using TryGetValue or null-conditional access
                string msg = responseObj.TryGetValue("msg", out var msgValue) ? msgValue?.ToString() ?? "" : "";
                responseObj["authenticated"] = false;

                // Check if there was any error message returned from GetHashedPassword
                if (msg == "Invalid Password" || msg == "User not found" || msg.Contains("we encountered an error"))
                { 
                    responseObj["status"] = string.IsNullOrEmpty(msg) ? "Authentication failed" : msg;
                    return responseObj;
                }

                // Safely check if "Password" exists before reading it
                if (!responseObj.TryGetValue("Password", out var storedHashObj) || storedHashObj == null)
                {
                    responseObj["status"] = "Invalid Password";
                    return responseObj;
                }

                string storedHash = storedHashObj.ToString()!;
                
                // Verify Password
                bool passwordMatch = VerifyPassword(password, storedHash);
                
                if (!passwordMatch)
                {
                    responseObj["status"] = "Invalid Password";
                    return responseObj;
                }
                
                // Success path
                responseObj["authenticated"] = true;
                responseObj["access_token"] = GenerateAccessToken();
                responseObj["status"] = "Username and password match";
                return responseObj;
        }
        
    } // end Verify Class
    
} // end namespace
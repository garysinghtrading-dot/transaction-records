using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
        
        public void VerifyLocally(string username, string password)
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
                // write data (Authentication Failed)
                WriteData(responseObj);
                return;
            }
            
            // cconvert DB value to string
            string StoredHashString = storedHash.ToString();

            
            // verify Password
            bool PasswordMatch = VerifyPassword(password, StoredHashString);
            
            if(!PasswordMatch)
            {
                responseObj["status"] = "Invalid Password";
                WriteData(responseObj);
                return;
            }
            
            // At this point the user has been able to be verified
            responseObj["authenticated"] = true;
            responseObj["access_token"] = GenerateAccessToken();
            responseObj["status"] = "Username and password match";
            
            WriteData(responseObj); // Print and done

        }
        
    } // end Verify Class
    
} // end namespace
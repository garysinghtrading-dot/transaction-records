using System;
//using System.Data.SQLite;
using Microsoft.Data.Sqlite;

namespace BankingApp
{
    public class Customer
    {
        private string FirstName;
        private string LastName;
        private int CustomerId;
        private string Password;
        private string Email;
        private string UserName;

        /*

        */
        public Customer(string fname, string lname, int cnum, string pword, string email, string uname)
        {
            FirstName = fname;
            LastName = lname;
            CustomerId = cnum;
            Password = pword;
            Email = email;
            UserName = uname;
        }

        public void AddNewCust()
        {
            using var conn = new SqliteConnection("Data Source=customers.db");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Customers (CustomerId, FirstName, LastName, Email, Password, UserName)
                VALUES ($id, $first, $last, $email, $pword, $uname);
            ";

            cmd.Parameters.AddWithValue("$id", CustomerId);
            cmd.Parameters.AddWithValue("$first", FirstName);
            cmd.Parameters.AddWithValue("$last", LastName);
            cmd.Parameters.AddWithValue("$email", Email);
            cmd.Parameters.AddWithValue("$pword", Password);
            cmd.Parameters.AddWithValue("$uname", UserName);

            cmd.ExecuteNonQuery();
        }
    }
}
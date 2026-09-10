using System;
using BankingApp;

namespace Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // evaluate command line argument
            if(args.Length == 0)
            {
                Console.WriteLine("Please provide a transaction argument selected");
                return;
            }
            var transaction = args[0];

            // Create new object of SendData Class
             SendTransactions STaws =  new SendTransactions();

            if(transaction == "AddNewCustomer")
            {
                if(args.Length < 4)
                {
                    Console.WriteLine("Need to enter firstname, lastname, customer id");
                    return;
                }
                string firstname = args[1];
                string lastname = args[2];
                int customerid = int.Parse(args[3]);
                var data = new {
                    FirstName = firstname,
                    LastName = lastname,
                    CustomerId = customerid
                };
                bool isSuccess= STaws.RecordTransaction(firstname, lastname, customerid, data, "add-new-customer").GetAwaiter().GetResult();
                if(isSuccess)
                    Console.WriteLine($"Customer {firstname} {lastname} successfully created");
                else
                    Console.WriteLine("Could not create customer successfully");
            } // end AddNewCustomer
            else if(transaction == "InitialDeposit")
            {
                if(args.Length < 5)
                {
                    Console.WriteLine("Need to enter firstname, lastname, customer id");
                    return;
                }
                string firstname = args[1];
                string lastname = args[2];
                int customerid = int.Parse(args[3]);
                int amount_ = int.Parse(args[4]);
                var data = new {
                    FirstName = firstname,
                    LastName = lastname,
                    CustomerId = customerid,
                    type = "Initial Deposit",
                    amount = amount_
                };
                bool isSuccess= STaws.RecordTransaction(firstname, lastname, customerid, data, "add-deposit").GetAwaiter().GetResult();
                if(isSuccess)
                    Console.WriteLine($"Customer {firstname} {lastname} Initial Deposit created successfully");
                else
                    Console.WriteLine("Could not create initial deposit");
            }
            else if(transaction == "Delete")
            {
                if(args.Length < 4)
                {
                    Console.WriteLine("Need to enter firstname, lastname, customer id");
                    return;
                }
                string firstname = args[1];
                string lastname = args[2];
                int customerid = int.Parse(args[3]);

                var data = new {
                    FirstName = firstname,
                    LastName = lastname,
                    CustomerId = customerid,
                };     
                bool isSuccess= STaws.RecordTransaction(firstname, lastname, customerid, data, "delete-customer").GetAwaiter().GetResult();
                if(isSuccess)
                    Console.WriteLine($"Customer {firstname} {lastname} Deleted Successfully");
                else
                    Console.WriteLine("Error deleting customer");           
            }
        } // end Main Method
    }
}
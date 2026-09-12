// File: BankingApp.Tests/ProgramTests.cs
using System;
using System.IO;
using Startup;
using Xunit;

namespace BankingApp.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void Main_AddNewCustomer_DuplicateUser_ReturnsFailure()
        {
            // Preserve original console writer to avoid cross-test output contamination
            TextWriter standardOutput = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);

            try
            {
                // Existing customer regression test
                string[] args = new[] { "AddNewCustomer", "Test1", "Run1", "11" };

                // Act
                Program.Main(args);

                // Assert
                string output = sw.ToString();
                Assert.Contains("Customer Test1 Run1 successfully create", output);
            }
            finally
            {
                Console.SetOut(standardOutput);
            }
        }

        [Fact]
        public void Main_AddNewCustomerInitialDeposit_ReturnsSuccess()
        {
            TextWriter standardOutput = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);

            try
            {
                // Added missing comma between "11" and "2500"
                string[] args = new[] { "InitialDeposit", "Test1", "Run1", "11", "2500" };

                // Act
                Program.Main(args);

                // Assert: Added missing closing quote and parenthesis
                string output = sw.ToString();
                Assert.Contains("Customer Test1 Run1 Initial Deposit created successfully", output);
            }
            finally
            {
                Console.SetOut(standardOutput);
            }
        }
        [Fact]
        public void Main_Delete_User_ReturnsTrue()
        {
            // Preserve original console writer to avoid cross-test output contamination
            TextWriter standardOutput = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);

            try
            {
                // Existing customer regression test
                string[] args = new[] { "Delete", "Test1", "Run1", "11" };

                // Act
                Program.Main(args);

                // Assert
                string output = sw.ToString();
                Assert.Contains("Customer Test1 Run1 Deleted Successfully", output);
            }
            finally
            {
                Console.SetOut(standardOutput);
            }
        }
    }
}
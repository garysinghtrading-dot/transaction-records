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
            // Capture Console output
            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Command-line arguments for existing customer ID 11
            string[] args = new[] { "AddNewCustomer", "Test1", "Run1", "11" };

            // Act
            Program.Main(args);

            // Assert: Verify failure message is printed
            string output = sw.ToString();
            Assert.Contains("Could not create customer successfully", output);
        }
    }
}
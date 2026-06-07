using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Domain;
using Domain.Protocols;
using Domain.Utilities;

namespace Unit_Tests
{
    [TestClass]
    public class TOTPUtilitiesTest
    {
        private const string ExampleSecret = "EXAMPLE";

        [TestMethod]
        public void TestMicrosoft()
        {
            string input = "otpauth://totp/Microsoft:user@hotmail.com?secret=" + ExampleSecret + "&issuer=Microsoft";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@hotmail.com", ExampleSecret, "Microsoft", account);
        }

        [TestMethod]
        public void TestGoogle()
        {
            string input = "otpauth://totp/Google%3Auser%40gmail.com?secret=" + ExampleSecret + "&issuer=Google";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@gmail.com", ExampleSecret, "Google", account);
        }

        [TestMethod]
        public void TestDropbox()
        {
            string input = "otpauth://totp/Dropbox:user@hotmail.com?secret=" + ExampleSecret + "&issuer=Dropbox";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@hotmail.com", ExampleSecret, "Dropbox", account);
        }

        [TestMethod]
        public void TestDiskStation()
        {
            string input = "otpauth://totp/Administrator@DiskStation?secret=" + ExampleSecret;
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("Administrator", ExampleSecret, "DiskStation", account);
        }

        [TestMethod]
        public void TestVersio()
        {
            string input = "otpauth://totp/12345?secret=" + ExampleSecret + "&issuer=Versio";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("12345", ExampleSecret, "Versio", account);
        }

        [TestMethod]
        public void TestDigitsParameter()
        {
            string input = "otpauth://totp/Blizzard:user@example.com?secret=" + ExampleSecret + "&issuer=Blizzard&digits=8";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@example.com", ExampleSecret, "Blizzard", account, 8);
        }

        [TestMethod]
        public void TestInvalidDigitsParameterDefaultsToSixDigits()
        {
            string input = "otpauth://totp/Example:user@example.com?secret=" + ExampleSecret + "&issuer=Example&digits=99";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@example.com", ExampleSecret, "Example", account);
        }

        private void Validate(string username, string secret, string service, Account account, byte digits = TOTP.DEFAULT_DIGITS)
        {
            Assert.IsNotNull(account);
            Assert.AreEqual(username, account.Username);
            Assert.AreEqual(secret, account.Secret);
            Assert.AreEqual(service, account.Service);
            Assert.AreEqual(digits, account.Digits);
        }
    }
}

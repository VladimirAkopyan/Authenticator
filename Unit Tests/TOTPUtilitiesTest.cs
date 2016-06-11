using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Domain;
using Domain.Utilities;

namespace Unit_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMicrosoft()
        {
            string input = "otpauth://totp/Microsoft:user@hotmail.com?secret=2FA662OCY6FLRKY5&issuer=Microsoft";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@hotmail.com", "2FA662OCY6FLRKY5", "Microsoft", account);
        }

        [TestMethod]
        public void TestGoogle()
        {
            string input = "otpauth://totp/Google%3Auser%40gmail.com?secret=uhs6vc47fttwgglvsyevlbjoczoyyeem&issuer=Google";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@gmail.com", "uhs6vc47fttwgglvsyevlbjoczoyyeem", "Google", account);
        }

        [TestMethod]
        public void TestDropbox()
        {
            string input = "otpauth://totp/Dropbox:user@hotmail.com?secret=EK6VFBVGIGJ4FX6KYONLBQSJH4&issuer=Dropbox";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("user@hotmail.com", "EK6VFBVGIGJ4FX6KYONLBQSJH4", "Dropbox", account);
        }

        [TestMethod]
        public void TestDiskStation()
        {
            string input = "otpauth://totp/Administrator@DiskStation?secret=A7LAL5KRDBZWRRHT";
            Account account = TOTPUtilities.UriToAccount(input);

            Validate("Administrator", "A7LAL5KRDBZWRRHT", "DiskStation", account);
        }

        private void Validate(string username, string secret, string service, Account account)
        {
            Assert.IsNotNull(account);
            Assert.AreEqual(username, account.Username);
            Assert.AreEqual(secret, account.Secret);
            Assert.AreEqual(service, account.Service);
        }
    }
}

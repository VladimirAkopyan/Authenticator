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

        private void Validate(string username, string secret, string service, Account account)
        {
            Assert.IsNotNull(account);
            Assert.AreEqual(username, account.Username);
            Assert.AreEqual(secret, account.Secret);
            Assert.AreEqual(service, account.Service);
        }
    }
}

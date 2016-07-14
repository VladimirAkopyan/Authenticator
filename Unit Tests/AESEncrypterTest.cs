using Encryption;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unit_Tests
{
    [TestClass]
    public class AESEncrypterTest
    {
        [TestMethod]
        public void TestEncryptAndDecrypt()
        {
            string input = "Test";

            AESEncrypter encrypter = GetAESEncrypter();
            string encrypted = encrypter.Encrypt(input);
            string decrypted = encrypter.Decrypt(encrypted);

            Assert.AreEqual(input, decrypted);
        }

        private AESEncrypter GetAESEncrypter()
        {
            return new AESEncrypter()
            {
                Password = "TestPassword",
                Salt = "TestSalt"
            };
        }
    }
}

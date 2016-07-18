using Encryption.Exceptions;
using System;
using System.Text;
using System.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Encryption
{
    public class AESEncrypter : IEncrypter
    {
        private string _salt;
        private string _password;

        public string Salt
        {
            set
            {
                _salt = value;
            }
        }

        public string Password
        {
            set
            {
                _password = value;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_salt) && !string.IsNullOrWhiteSpace(_password);
            }
        }

        public string Encrypt(string unencryptedText)
        {
            // Generate a key and IV from the password and salt
            IBuffer aesKeyMaterial;
            IBuffer iv;
            uint iterationCount = 10000;
            GenerateKeyMaterial(_password, _salt, iterationCount, out aesKeyMaterial, out iv);

            IBuffer plainText = CryptographicBuffer.ConvertStringToBinary(unencryptedText, BinaryStringEncoding.Utf8);

            // Setup an AES key, using AES in CBC mode and applying PKCS#7 padding on the input
            SymmetricKeyAlgorithmProvider aesProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            CryptographicKey aesKey = aesProvider.CreateSymmetricKey(aesKeyMaterial);

            // Encrypt the data and convert it to a Base64 string
            IBuffer encrypted = CryptographicEngine.Encrypt(aesKey, plainText, iv);
            return CryptographicBuffer.EncodeToBase64String(encrypted);
        }

        public string Decrypt(string encryptedText)
        {
            // Generate a key and IV from the password and salt
            IBuffer aesKeyMaterial;
            IBuffer iv;
            uint iterationCount = 10000;
            GenerateKeyMaterial(_password, _salt, iterationCount, out aesKeyMaterial, out iv);

            // Setup an AES key, using AES in CBC mode and applying PKCS#7 padding on the input
            SymmetricKeyAlgorithmProvider aesProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            CryptographicKey aesKey = aesProvider.CreateSymmetricKey(aesKeyMaterial);

            // Convert the base64 input to an IBuffer for decryption
            IBuffer ciphertext = CryptographicBuffer.DecodeFromBase64String(encryptedText);

            // Decrypt the data and convert it back to a string
            IBuffer decrypted = CryptographicEngine.Decrypt(aesKey, ciphertext, iv);
            byte[] decryptedArray = decrypted.ToArray();
            return Encoding.UTF8.GetString(decryptedArray, 0, decryptedArray.Length);
        }

        private static void GenerateKeyMaterial(string password, string salt, uint iterationCount, out IBuffer keyMaterial, out IBuffer iv)
        {
            // Setup KDF parameters for the desired salt and iteration count
            IBuffer saltBuffer = CryptographicBuffer.ConvertStringToBinary(salt, BinaryStringEncoding.Utf8);
            KeyDerivationParameters kdfParameters = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, iterationCount);

            // Get a KDF provider for PBKDF2, and store the source password in a Cryptographic Key
            KeyDerivationAlgorithmProvider kdf = KeyDerivationAlgorithmProvider.OpenAlgorithm(KeyDerivationAlgorithmNames.Pbkdf2Sha256);
            IBuffer passwordBuffer = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);
            CryptographicKey passwordSourceKey = kdf.CreateKey(passwordBuffer);

            // Generate key material from the source password, salt, and iteration count.  Only call DeriveKeyMaterial once,
            // since calling it twice will generate the same data for the key and IV.
            int keySize = 256 / 8;
            int ivSize = 128 / 8;
            uint totalDataNeeded = (uint)(keySize + ivSize);
            IBuffer keyAndIv = CryptographicEngine.DeriveKeyMaterial(passwordSourceKey, kdfParameters, totalDataNeeded);

            // Split the derived bytes into a seperate key and IV
            byte[] keyMaterialBytes = keyAndIv.ToArray();
            keyMaterial = WindowsRuntimeBuffer.Create(keyMaterialBytes, 0, keySize, keySize);
            iv = WindowsRuntimeBuffer.Create(keyMaterialBytes, keySize, ivSize, ivSize);
        }
    }
}

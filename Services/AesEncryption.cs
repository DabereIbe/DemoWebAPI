using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DemoWebAPI.Services
{
    public class AesEncryption
    {
        private byte[] Key = Encoding.UTF8.GetBytes("mysmallkey1234551298765134567890");
        private byte[] Iv = Encoding.UTF8.GetBytes("mysmallkey123456");

        public byte[] Encrypt<T>(T obj)
        {
            // Generate a random encryption key and initialization vector
            byte[] key = Key; // 256-bit key
            byte[] iv = Iv;   // 128-bit IV

            string serializedObject = SerializeObject(obj);

            // Encrypt the message
            byte[] encrypted = EncryptObject(Encoding.UTF8.GetBytes(serializedObject), key, iv);
            
            return encrypted;
        }
        
        public byte[] Encrypt(string text)
        {
            // Generate a random encryption key and initialization vector
            byte[] key = Key; // 256-bit key
            byte[] iv = Iv;   // 128-bit IV

            

            // Encrypt the message
            byte[] encrypted = EncryptObject(Encoding.UTF8.GetBytes(text), key, iv);
            
            return encrypted;
        }

        public T Decrypt<T>(byte[] encryptedText)
        {
            // Generate a random encryption key and initialization vector
            byte[] key = Key; // 256-bit key
            byte[] iv = Iv;   // 128-bit IV

            // Encrypt the message
            byte[] decrypted = DecryptObject(encryptedText, key, iv);

            T decryptedObject = DeserializeObject<T>(decrypted);

            return decryptedObject;
        }
        
        public string Decrypt(byte[] encryptedText)
        {
            // Generate a random encryption key and initialization vector
            byte[] key = Key; // 256-bit key
            byte[] iv = Iv;   // 128-bit IV

            // Encrypt the message
            string decrypted = DecryptStringFromBytes_Aes(encryptedText, key, iv);

            return decrypted;
        }

        static string SerializeObject<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        static T DeserializeObject<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes));
        }

        static byte[] EncryptObject(byte[] plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode explicitly

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plainText, 0, plainText.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        static byte[] DecryptObject(byte[] cipherText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode explicitly

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(cipherText, 0, cipherText.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray();
                    }
                }
            }
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode explicitly

                // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        static byte[] GenerateRandomKey(int length)
        {
            byte[] key = new byte[length];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return key;
        }
    }
}

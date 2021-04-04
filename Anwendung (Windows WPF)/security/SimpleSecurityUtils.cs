using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.security
{
    static class SimpleSecurityUtils
    {

        /// <summary>
        /// Encryptes the given bytes using aes-cbc
        /// </summary>
        /// <param name="data">The raw data that shall be encrypted</param>
        /// <param name="key">The AES-Key (32-Bytes or 256 bit). If the length of the key is unknown, throw the original into the sha256 algorithm before using this function.</param>
        /// <param name="iv">The inital vector to randomize the encryption (16 Byte or 128 bit). If the length of the iv is unknown, throw the original into a md5 algorithm before using this function.</param>
        /// <returns>Null if anything went wrong (Maybe a key or iv with a different length form the required have been given?); otherwise an array with the encrypted bytes</returns>
        public static byte[] EncryptAES(byte[] data,byte[] key,byte[] iv)
        {
            try
            {
                RijndaelManaged SymmetricKey = new RijndaelManaged
                {
                    Mode = CipherMode.CBC
                };
                byte[] CipherTextBytes = null;
                using (ICryptoTransform Encryptor = SymmetricKey.CreateEncryptor(key, iv))
                {
                    using (MemoryStream MemStream = new MemoryStream())
                    {
                        using (CryptoStream CryptoStream = new CryptoStream(MemStream, Encryptor, CryptoStreamMode.Write))
                        {
                            CryptoStream.Write(data, 0, data.Length);
                            CryptoStream.FlushFinalBlock();
                            CipherTextBytes = MemStream.ToArray();
                        }
                    }
                }
                SymmetricKey.Clear();
                return CipherTextBytes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Decryptes the given bytes using aes-cbc
        /// </summary>
        /// <param name="data">The encrypted data</param>
        /// <param name="key">The AES-Key (32-Bytes or 256 bit). If the length of the key is unknown, throw the original into the sha256 algorithm before using this function.</param>
        /// <param name="iv">The inital vector to randomize the encryption (16 Byte or 128 bit). If the length of the iv is unknown, throw the original into a md5 algorithm before using this function.</param>
        /// <returns>Null if the key or iv were wrong or if anything else went wrong (Maybe a key or iv with a different length form the required have been given?); otherwise an array with the decrypted bytes</returns>
        public static byte[] DecryptAES(byte[] data,byte[] key,byte[] iv)
        {
            try
            {
                RijndaelManaged SymmetricKey = new RijndaelManaged
                {
                    Mode = CipherMode.CBC
                };
                byte[] PlainTextBytes = new byte[data.Length];
                int ByteCount = 0;
                using (ICryptoTransform Decryptor = SymmetricKey.CreateDecryptor(key, iv))
                {
                    using (MemoryStream MemStream = new MemoryStream(data))
                    {
                        using (CryptoStream CryptoStream = new CryptoStream(MemStream, Decryptor, CryptoStreamMode.Read))
                        {

                            ByteCount = CryptoStream.Read(PlainTextBytes, 0, PlainTextBytes.Length);
                        }
                    }
                }
                SymmetricKey.Clear();

                byte[] x = new byte[ByteCount];
                Array.Copy(PlainTextBytes, 0, x, 0, ByteCount);
                return x;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Hashes the given data using the SHA256-hash algorithm.
        /// </summary>
        /// <param name="data">The raw data to hash</param>
        /// <returns>A byte-array with 256 bit (32 bytes).</returns>
        public static byte[] HashSHA256(byte[] data)
        {
            // Create an instance of the hash algorithm 
            using (SHA256 hash = SHA256.Create())
                // Computes the hash
                return hash.ComputeHash(data);
        }

        /// <summary>
        /// Hashes the given data using the SHA256-hash algorithm.
        /// </summary>
        /// <param name="data">The raw data to hash</param>
        /// <returns>A byte-array with 256 bit (32 bytes).</returns>
        public static byte[] HashMD5(byte[] data)
        {
            // Create an instance of the hash algorithm 
            using (MD5 hash = MD5.Create())
                // Computes the hash
                return hash.ComputeHash(data);
        }

        /// <summary>
        /// Loads rsa-parameters from a json-object (Using a program-consistent spec)
        /// </summary>
        /// <param name="obj">The raw json object from the loaded config</param>
        /// <returns>The loaded rsa-parameters</returns>
        /// <exception cref="Exception">Can throw an exception while loading. (Parameters not given, not a b64 string, etc.)</exception>
        public static RSAParameters LoadRSAFromJson(JObject obj)
        {
            // Short function to load rsa-parameters
            byte[] TryLoad(string name) => Convert.FromBase64String((string)obj[name]);

            return new RSAParameters()
            {
                D = TryLoad("d"),
                DP = TryLoad("dp"),
                DQ = TryLoad("dq"),
                Modulus = TryLoad("modulus"),
                InverseQ = TryLoad("inverseq"),
                Exponent = TryLoad("exponent"),
                P = TryLoad("p"),
                Q = TryLoad("q")
            };
        }

        /// <summary>
        /// Saves the given rsa-parameters to a jobject (Using a program-consistent spec)
        /// </summary>
        /// <param name="rsaKey">The rsa-parameters that shall be saved</param>
        /// <returns>A JObject that can be converted back into rsa-parameters using the LoadFromObject method.</returns>
        public static JObject SaveRSAToJson(RSAParameters rsaKey)
        {
            // Short function to save a byte[] to a base64 string
            string Save(byte[] bytes) => Convert.ToBase64String(bytes);

            return new JObject
            {
                ["d"] = Save(rsaKey.D),
                ["dp"] = Save(rsaKey.DP),
                ["dq"] = Save(rsaKey.DQ),
                ["modulus"] = Save(rsaKey.Modulus),
                ["inverseq"] = Save(rsaKey.InverseQ),
                ["exponent"] = Save(rsaKey.Exponent),
                ["p"] = Save(rsaKey.P),
                ["q"] = Save(rsaKey.Q)
            };
        }

    }
}

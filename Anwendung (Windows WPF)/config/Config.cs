using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.config;
using Pl_Covid_19_Anmeldung.security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace projektlabor.noah.planmeldung
{
    class Config
    {
        /// <summary>
        /// Port on the remote server on which the backend is listening
        /// </summary>
        public int Port;

        /// <summary>
        /// Hostaddress/Ip of the remote server
        /// </summary>
        public string Host;

        /// <summary>
        /// Key of this device that is used to authenticate on the remote server
        /// </summary>
        public RSAParameters PrivateKey;

        /// <summary>
        /// Loads the given config-file, decryptes it using the passed key and loads it as a config object
        /// </summary>
        /// <param name="filePath">The path to the configuration file.</param>
        /// <param name="key">The key to decrypt those bytes</param>
        /// <returns>The fully loaded config</returns>
        /// <exception cref="ConfigInvalidKeyException">If the given key couldn't decrypt the config</exception>
        /// <exception cref="ConfigCurruptedException">If the config seems to be currupted</exception>
        /// <exception cref="Exception">All exception that can be passed by File.ReadAllBytes using the filepath.</exception>
        public static Config LoadConfig(string filePath,string key)
        {
            // Reads the content of the passed config file
            byte[] rawData = File.ReadAllBytes(filePath);

            // Generates the actual key and iv from the previous plain text key
            byte[] aesKey = SimpleSecurityUtils.HashSHA256(Encoding.UTF8.GetBytes(key));
            byte[] aesIv = SimpleSecurityUtils.HashMD5(Encoding.UTF8.GetBytes(key));

            // Tries to decrypt the config
            byte[] decrypted = SimpleSecurityUtils.DecryptAES(rawData, aesKey, aesIv);

            // Checks if the decryption failed
            if (decrypted == null)
                throw new ConfigInvalidKeyException();

            try
            {
                // The config that will be created
                Config cfg = new Config();

                // Tries to parse the config from json
                JObject json = JObject.Parse(Encoding.UTF8.GetString(decrypted));

                // Gets all values
                cfg.Port = (int)json["port"];
                cfg.Host = (string)json["host"] ?? string.Empty;
                cfg.PrivateKey = SimpleSecurityUtils.LoadRSAFromJson((JObject)json["rsa"]);

                // Returns the fully loaded config
                return cfg;
            }
            catch
            {
                throw new ConfigCurruptedException(Encoding.UTF8.GetString(decrypted));
            }
        }

        /// <summary>
        /// Saves the config file encrypted using the passed key
        /// </summary>
        /// <param name="filePath">The path to the file where to save the configuration</param>
        /// <param name="key">The key for encrypting the file</param>
        /// <exception cref="ConfigException">Something went wrong while encryption (Unknown)</exception>
        /// <exception cref="Exception">All exception that can be passed on by File.WriteAllBytes called with the filePath</exception>
        public void SaveConfig(string filePath,string key)
        {
            // Generates the object that will be saved as the config file
            JObject json = new JObject()
            {
                ["port"] = this.Port,
                ["host"] = this.Host,
                ["rsa"] = SimpleSecurityUtils.SaveRSAToJson(this.PrivateKey)
            };

            // Generates the actual key and iv from the previous plain text key
            byte[] aesKey = SimpleSecurityUtils.HashSHA256(Encoding.UTF8.GetBytes(key));
            byte[] aesIv = SimpleSecurityUtils.HashMD5(Encoding.UTF8.GetBytes(key));

            // Encryptes the config-json object
            byte[] encData = SimpleSecurityUtils.EncryptAES(Encoding.UTF8.GetBytes(json.ToString()), aesKey, aesIv);

            // Checks if the encryption failed
            if (encData == null)
                // Unknown exception
                throw new ConfigException();

            // Writes all data to the file
            File.WriteAllBytes(filePath, encData);
        }
        
    }
}

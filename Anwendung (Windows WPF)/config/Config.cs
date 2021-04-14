using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung;
using Pl_Covid_19_Anmeldung.config;
using Pl_Covid_19_Anmeldung.security;
using Pl_Covid_19_Anmeldung.windows.dialogs;
using Pl_Covid_19_Anmeldung.windows.requests;
using projektlabor.noah.planmeldung.Properties.langs;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace projektlabor.noah.planmeldung
{
    public class Config
    {

        // Reference to the program-logger
        private static readonly Logger log = new Logger("Config");

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
        /// Saves the config file encrypted using the passed key
        /// </summary>
        /// <param name="filePath">The path to the file where to save the configuration</param>
        /// <param name="key">The key for encrypting the file</param>
        /// <exception cref="ConfigException">Something went wrong while encryption (Unknown)</exception>
        /// <exception cref="Exception">All exception that can be passed on by File.WriteAllBytes called with the filePath</exception>
        public void SaveConfig(string filePath, string key)
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

        /// <summary>
        /// Loads the given config-file, decryptes it using the passed key and loads it as a config object
        /// </summary>
        /// <param name="filePath">The path to the configuration file.</param>
        /// <param name="key">The key to decrypt those bytes</param>
        /// <returns>The fully loaded config</returns>
        /// <exception cref="ConfigInvalidKeyException">If the given key couldn't decrypt the config</exception>
        /// <exception cref="ConfigCurruptedException">If the config seems to be currupted</exception>
        /// <exception cref="Exception">All exception that can be passed by File.ReadAllBytes using the filepath.</exception>
        public static Config LoadConfig(string filePath, string key)
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
        /// Opens a serias of windows that ask the user for a password to open a config file
        /// </summary>
        /// <param name="OnReceive">Callback that executes once the config got loaded successfull. Contains a bool that says if the config has been created new (true) or if the config has been loaded and therewas existed (false). Contains a string that represents the config password, used to de/encrypt the config.</param>
        /// <param name="OnCancel">Callback that executes once the user cancels opening of the config</param>
        /// <returns>Null if the user cancled the config-request; otherwise the loaded or newly created config</returns>
        public static void GetConfigFromUser(Action<bool, Config,string> OnReceive, Action OnCancel)
        {
            log.Debug("Starting to request the config (From the user)");

            // Checks if a config file exists
            if (!File.Exists(PLCA.CONFIG_PATH))
            {
                log.Debug("Config does not exist, asking to create a new one.");

                // Creates a new window to ask the user for further instructions
                var askNew = new YesNoWindow(
                    Lang.config_notexisting_title,
                    OnCreateNewConfig,
                    OnCancel,
                    Lang.config_notexisting_yes,
                    Lang.config_notexisting_no,
                    Lang.config_notexisting_title,
                    Lang.config_notexisting_info
                );

                // Opens the window
                askNew.ShowDialog();
                return;
            }

            // Creates the window to ask for the config-password
            var askPass = new TextinputWindow(
                Lang.config_pass_title,
                Lang.config_pass_title,
                OnReceivePassword,
                OnCancel,
                Lang.config_pass_ok,
                Lang.config_pass_cancle);

            // Opens the window
            askPass.ShowDialog();

            // Executes once the user has submitted a password
            void OnReceivePassword(string password)
            {
                try
                {
                    log
                        .Debug("Received password, creating new config.")
                        .Critical("Password="+password);

                    // Tries to load the config and sends the received config
                    OnReceive(false,LoadConfig(PLCA.CONFIG_PATH, password),password);
                }
                catch (ConfigCurruptedException e)
                {
                    log
                        .Warn("The previous config could be decrypted, but is currupted")
                        .Critical("Raw previous content="+e.RawInput);

                    // The config could be decrypted but is currupted
                    // Creates a new window to ask the user for further instructions
                    var askNew = new YesNoWindow(
                        Lang.config_currupted_title,
                        OnCreateNewConfig,
                        OnCancel,
                        Lang.config_currupted_yes,
                        Lang.config_currupted_no,
                        Lang.config_currupted_title,
                        Lang.config_currupted_info,
                        () => Clipboard.SetText(e.RawInput),
                        Lang.config_currepted_copy
                    );

                    // Opens the window
                    askNew.ShowDialog();
                }
                catch (ConfigInvalidKeyException)
                {
                    log.Debug("User has specified an invalid password.");

                    // Creates a window to inform the user
                    var ackWin = new AcknowledgmentWindow(
                        Lang.config_wrongpw_title,
                        Lang.config_wrongpw_title,
                        () => GetConfigFromUser(OnReceive, OnCancel),
                        Lang.config_wrongpw_retry
                    );

                    // Opens the window
                    ackWin.ShowDialog();
                }
                catch (Exception e)
                {
                    log
                        .Debug("Unknown error occurred (Maybe a file error with permissions?)")
                        .Critical(e);

                    // Creates a window to inform the user
                    var ackWin = new AcknowledgmentWindow(
                        Lang.config_loaderr_title,
                        e.Message,
                        OnCancel,
                        Lang.config_loaderr_close
                    );

                    // Opens the window
                    ackWin.ShowDialog();
                }
            }

            // Executes once the user wants to create a new config (Because the old one got destroyed)
            void OnCreateNewConfig()
            {
                log.Debug("Asking for the password of the new config");

                // Asks for the new password
                var askPass2 = new TextinputWindow(
                    Lang.config_new_pass_title,
                    Lang.config_new_pass_text,
                    OnSubmitNewPassword,
                    OnCancel,
                    Lang.config_pass_ok,
                    Lang.config_pass_cancle
                );

                // Shows the window
                askPass2.ShowDialog();
            }

            // Executes once the user has decided the new password
            void OnSubmitNewPassword(string password)
            {
                // Creates a config with the default parameters
                Config cfg = new Config
                {
                    Host = string.Empty,
                    Port = 0,
                    PrivateKey = new RSAParameters()
                    {
                        D = new byte[0],
                        DP = new byte[0],
                        DQ = new byte[0],
                        Exponent = new byte[0],
                        InverseQ = new byte[0],
                        Modulus = new byte[0],
                        P = new byte[0],
                        Q = new byte[0]
                    }
                };

                try
                {
                    log
                        .Debug("Saving new config")
                        .Critical("New Password="+password);

                    // Tries to save the config
                    cfg.SaveConfig(PLCA.CONFIG_PATH, password);

                    log.Debug("Successfully saved the new config");

                    // Returns the new config
                    OnReceive(true,cfg,password);
                }
                catch (Exception e)
                {
                    log
                        .Debug("Error saving the new config. Maybe a file-error with permissions?")
                        .Critical(e);

                    // Creates the warning window
                    var ackWin = new AcknowledgmentWindow(
                        Lang.config_new_error,
                        e.Message,
                        OnCancel,
                        Lang.config_new_error_button
                    );

                    // Opens the new window
                    ackWin.ShowDialog();
                }
            }
        }
    }
}

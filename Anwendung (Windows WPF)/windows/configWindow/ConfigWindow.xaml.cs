using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.security;
using Pl_Covid_19_Anmeldung.windows.dialogs;
using Pl_Covid_19_Anmeldung.windows.requests;
using projektlabor.noah.planmeldung;
using projektlabor.noah.planmeldung.Properties.langs;
using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace Pl_Covid_19_Anmeldung.windows.configWindow
{
    /// <summary>
    /// Interaction logic for ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        // Holds the loaded config (Not active config, this one is editable (Throwaway))
        private Config cfg;

        // Stores the password of the config
        private string password;

        public ConfigWindow(Config cfg,string password)
        {
            InitializeComponent();
            this.cfg = cfg;
            this.password = password;

            // Sets the values
            this.FieldPort.Text = cfg.Port.ToString();
            this.FieldHost.Text = cfg.Host;
            this.FieldRSA.Text = SimpleSecurityUtils.SaveRSAToJson(cfg.PrivateKey).ToString(Formatting.None);

            // Disables the save button
            this.buttonSave.IsEnabled = false;
        }

        /// <summary>
        /// Displays an error window with the given message
        /// </summary>
        /// <param name="title">The title of the window (An if no message got passed, the message as well)</param>
        /// <param name="message">The message to show to the user. If left out the title will be displayed</param>
        private void ShowError(string title,string message = null)
        {
            // Creates the window
            var win = new AcknowledgmentWindow(
                title,
                message ?? title,
                null,
                Lang.config_save_button
            );

            // Displays the window
            win.ShowDialog();
        }

        /// <summary>
        /// Handler for when the user typed on any text element
        /// </summary>
        private void OnTypeHandler(TextChangedEventArgs _)
        {
            // Makes the save and reset button available
            this.buttonSave.IsEnabled = true;
        }

        /// <summary>
        /// Handler that executes when the cancel buttons get's clicked
        /// </summary>
        private void OnButtonCancelClicked(object _, RoutedEventArgs _2) => this.Close();

        private void OnButtonSaveClicked(object _, RoutedEventArgs _2)
        {
            // Checks if the port couldn't be passed
            if(!int.TryParse(this.FieldPort.Text,out int port))
            {
                this.ShowError(Lang.config_save_port);
                return;
            }
            this.cfg.Port = port;

            try
            {
                // Tries to load the private key
                this.cfg.PrivateKey = SimpleSecurityUtils.LoadRSAFromJson(JObject.Parse(this.FieldRSA.Text));
            }
            catch
            {
                this.ShowError(Lang.config_save_key);
                return;
            }

            // Sets the host
            this.cfg.Host = this.FieldHost.Text;

            try
            {
                // Tries to save the config
                this.cfg.SaveConfig(PLCA.CONFIG_PATH, this.password);

                // Updates the main config
                PLCA.LOADED_CONFIG = this.cfg;

                // Closes the window
                this.Close();
            }
            catch(Exception ex)
            {
                // Shows the error
                this.ShowError(Lang.config_save_unknown,ex.Message);
            }
        }

        /// <summary>
        /// Handler that execut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonNewPasswordClicked(object sender, RoutedEventArgs e)
        {
            // Asks the user for the new password
            var askWin = new TextinputWindow(
                Lang.config_newpw_title,
                Lang.config_newpw_title,
                OnSubmitPassword,
                null,
                Lang.config_newpw_yes,
                Lang.config_newpw_no
            );

            // Opens the window
            askWin.ShowDialog();

            // Executes once the user submits a new password
            void OnSubmitPassword(string password)
            {
                // Sets the new password
                this.password = password;

                // Enables the save button
                this.buttonSave.IsEnabled = true;
            }
        }
    }
}

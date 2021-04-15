using System.Threading.Tasks;
using System.Windows;
using projektlabor.noah.planmeldung.Properties.langs;
using Pl_Covid_19_Anmeldung.connection.requests;
using Pl_Covid_19_Anmeldung;

namespace projektlabor.noah.planmeldung.windows
{
    public partial class MainWindow : Window
    {

        #region Event-handlers

        /// <summary>
        /// Executes when the user checks or unchecks the checkbox to accept the rules.
        /// 
        /// Enables or diables the button in that context
        /// </summary>
        private void OnRegistrationRuleCheckboxChanged(object sender, RoutedEventArgs e) => this.ButtonRegisterRegister.IsEnabled = this.CheckboxRegAcceptRules.IsChecked.Value;

        /// <summary>
        /// Executes when the user clicks the register button
        /// </summary>
        private void OnRegistrationButtonClicked(object sender, RoutedEventArgs e)
        {
            // Checks if all register field match their case
            if (!this.FormRegister.UpdateFields())
                return;

            // Displays the loading
            this.DisplayLoading(Lang.main_register_loading);

            // Gets the inserted user
            var user = this.FormRegister.UserInput;

            Task.Run(() =>
            {
                // Creates the login request
                var request = new RegisterUserRequest()
                {
                    OnErrorIO = this.OnRequestErrorIO,
                    OnNonsenseError = this.OnRequestErrorNonsense,
                    OnRFIDAlreadyUsed = OnUsedRFID,
                    OnUserAlreadyExists = OnUsedName,
                    OnSuccess = OnSuccess
                };

                // Reference to the config
                var cfg = PLCA.LOADED_CONFIG;

                // Starts the request
                request.DoRequest(cfg.Host, cfg.Port, cfg.PrivateKey, user);

                // Executes if the user got registered successfully
                void OnSuccess(int uid) => this.Dispatcher.Invoke(() =>
                {
                    // Resets the form
                    this.FormRegister.ResetForm();
                    this.CheckboxRegAcceptRules.IsChecked = false;
                    this.ButtonRegisterRegister.IsEnabled = false;

                    // Displays the info
                    this.DisplayInfo(
                        Lang.main_register_success_title,
                        Lang.main_register_success_text,
                        this.CloseOverlay,
                        Lang.main_popup_close,
                        false
                    );
                });

                // Executes if the rfid is in use already
                void OnUsedRFID()
                {
                    // Displays the info
                    this.DisplayInfo(
                        Lang.main_register_error_title,
                        Lang.main_register_error_rfid,
                        this.CloseOverlay,
                        Lang.main_popup_close
                    );
                };

                // Executes if the name is in use already
                void OnUsedName() => this.Dispatcher.Invoke(() =>
                {
                    // Displays the info
                    this.DisplayInfo(
                        Lang.main_register_error_title,
                        Lang.main_register_error_name,
                        this.CloseOverlay,
                        Lang.main_popup_close
                    );
                });

            });
        }

        /// <summary>
        /// Executes when the register form gets reset
        /// </summary>
        private void OnRegistrationResetClicked(object sender, RoutedEventArgs e)
        {
            // Updates the button and checkboxs
            this.ButtonRegisterRegister.IsEnabled = false;
            this.CheckboxRegAcceptRules.IsChecked = false;

            // Resets the form
            this.FormRegister.ResetForm();
        }

        #endregion
    }
}

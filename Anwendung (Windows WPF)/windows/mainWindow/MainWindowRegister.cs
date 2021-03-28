using System.Threading.Tasks;
using System.Windows;
using projektlabor.noah.planmeldung.datahandling;
using projektlabor.noah.planmeldung.Properties.langs;
using MySql.Data.MySqlClient;

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
            
            // TODO: Restart register user request
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

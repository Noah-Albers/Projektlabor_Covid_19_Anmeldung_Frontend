using Pl_Covid_19_Anmeldung;
using Pl_Covid_19_Anmeldung.connection.requests;
using projektlabor.noah.planmeldung.datahandling.entities;
using projektlabor.noah.planmeldung.Properties.langs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace projektlabor.noah.planmeldung.windows
{
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Holds the currently selected user
        /// </summary>
        private SimpleUserEntity selectedLoginUser;

        /// <summary>
        /// Holds all form field elements that are used at the login form.
        /// This is used to autodelete all data from these forms.
        /// </summary>
        private readonly TextBox[] loginFieldGroup;

        #region Event-handlers

        /// <summary>
        /// Executes when the user selects another user with the login form
        /// </summary>
        /// <param name="user">The selected user</param>
        private void OnLoginUserSelectSelect(SimpleUserEntity user) => Task.Run(() =>
        {
            // Displays the loading
            this.DisplayLoading(Lang.main_login_select_loading);

            // Creates the request
            var request = new GetStatusRequest()
            {
                OnErrorIO = this.OnRequestErrorIO,
                OnNonsenseError = this.OnRequestErrorNonsense,
                OnUserNotFound = OnUserNotFound,
                OnUserLoggedIn = OnUpdate,
                OnUserNotLoggedIn = ()=>OnUpdate(null)
            };

            // Reference to the config
            var cfg = PLCA.LOADED_CONFIG;

            // Starts the request
            request.DoRequest(cfg.Host, cfg.Port, cfg.PrivateKey,user.Id.Value);

            // Executes if the user could not be found?!
            void OnUserNotFound() =>
                // Displays an info window
                this.DisplayInfo(
                    Lang.main_login_get_error_not_found_title,
                    Lang.main_login_get_error_not_found_text,
                    this.CloseOverlay,
                    Lang.main_login_error_button_ok
                 );

            // Executes once a valid response has been received
            // Ts is null if the user is not logged in
            void OnUpdate(TimespentEntity ts) => this.Dispatcher.Invoke(() =>
            {
                // Checks if no unclosed sessions have been found
                if (ts == null)
                    // Creates a new login
                    ts = new TimespentEntity()
                    {
                        Start = DateTime.Now
                    };
                else
                    // Updates the stop-time
                    ts.Stop = DateTime.Now;

                // Resets the form
                this.LoginResetForm();

                // Displays the login form
                this.LoginDisplayUser(user, ts);

                // Closes the loading
                this.CloseOverlay();
            });
        });

        /// <summary>
        /// Executes when the user clicks the login button
        /// </summary>
        private void OnLoginButtonClick(object sender, RoutedEventArgs e) => Task.Run(() =>
        {
            // Displays the loading animation
            this.DisplayLoading(Lang.main_login_loading);

            // Creates the login request
            var request = new LoginRequest()
            {
                OnErrorIO = this.OnRequestErrorIO,
                OnNonsenseError = this.OnRequestErrorNonsense,
                OnUnauthorizedError = OnUnauth,
                OnUserNotFound = OnUserNotFound,
                OnSuccessfullLogin = OnSuccess
            };

            // Reference to the config
            var cfg = PLCA.LOADED_CONFIG;

            // Starts the request
            request.DoRequest(cfg.Host, cfg.Port, cfg.PrivateKey, this.selectedLoginUser.Id.Value);

            // Executes if the login was successful
            void OnSuccess() => this.Dispatcher.Invoke(() =>
            {
                // Closes the animation
                this.CloseOverlay();
                // Clears the form
                this.LoginResetForm();
            });

            // Executes if the user couldn't be found?!
            void OnUserNotFound() => this.Dispatcher.Invoke(() =>
            {
                // Closes the animation
                this.CloseOverlay();
                // Clears the form
                this.LoginResetForm();
                // Displays the info
                this.DisplayInfo(
                    Lang.main_login_login_error_not_found_title,
                    Lang.main_login_login_error_not_found_text,
                    this.CloseOverlay,
                    Lang.main_login_error_button_ok
                 );
            });

            // Executes if the user is still loged in?!
            void OnUnauth() => this.Dispatcher.Invoke(() =>
            {
                // Closes the animation
                this.CloseOverlay();
                // Clears the form
                this.LoginResetForm();
                // Displays the info
                this.DisplayInfo(
                    Lang.main_login_login_error_loggedin_title,
                    Lang.main_login_login_error_loggedin_text,
                    this.CloseOverlay,
                    Lang.main_login_error_button_ok
                 );
            });
        });

        /// <summary>
        /// Executes when the user clicks the logout button
        /// </summary>
        private void OnLogoutButtonClick(object sender, RoutedEventArgs e) => Task.Run(() =>
        {
            // Displays the loading animation
            this.DisplayLoading(Lang.main_login_loading_logout);

            // Creates the login request
            var request = new LogoutRequest()
            {
                OnErrorIO = this.OnRequestErrorIO,
                OnNonsenseError = this.OnRequestErrorNonsense,
                OnSuccessfullLogout = OnSuccess,
                OnUnauthorizedError = OnUnauth,
                OnUserNotFound = OnUserNotFound
            };

            // Reference to the config
            var cfg = PLCA.LOADED_CONFIG;

            // Starts the request
            request.DoRequest(cfg.Host, cfg.Port, cfg.PrivateKey, this.selectedLoginUser.Id.Value);

            // Executes if the login was successful
            void OnSuccess() => this.Dispatcher.Invoke(() =>
            {
                // Closes the animation
                this.CloseOverlay();
                // Clears the form
                this.LoginResetForm();
            });

            // Executes if the user couldn't be found?!
            void OnUserNotFound() => this.Dispatcher.Invoke(() =>
            {
                // Closes the animation
                this.CloseOverlay();
                // Clears the form
                this.LoginResetForm();
                // Displays the info
                this.DisplayInfo(
                    Lang.main_login_logout_error_not_found_title,
                    Lang.main_login_logout_error_not_found_text,
                    this.CloseOverlay,
                    Lang.main_login_error_button_ok
                 );
            });

            // Executes if the user is still loged in?!
            void OnUnauth() => this.Dispatcher.Invoke(() =>
            {
                // Closes the animation
                this.CloseOverlay();
                // Clears the form
                this.LoginResetForm();
                // Displays the info
                this.DisplayInfo(
                    Lang.main_login_logout_error_loggedout_title,
                    Lang.main_login_logout_error_loggedout_text,
                    this.CloseOverlay,
                    Lang.main_login_error_button_ok
                 );
            });
        });

        /// <summary>
        /// Executes when the user clicks the button that should delete the login form data
        /// </summary>
        private void OnLoginDeleteButtonClick(object sender, RoutedEventArgs e) => this.LoginResetForm();

        #endregion

        #region Actions

        /// <summary>
        /// Tries to login the user using his rfid
        /// </summary>
        /// <param name="rfid">The rfid</param>
        private void LoginUserFromRFID(string rfid) => Task.Run(() =>
        {
            // Shows the loading window
            this.DisplayLoading(Lang.main_rfid_loading);

            // Creates the login request
            var request = new LoginRFIDRequest()
            {
                OnErrorIO = this.OnRequestErrorIO,
                OnNonsenseError = this.OnRequestErrorNonsense,
                OnSuccess = OnSuccess,
                OnUserNotFound = OnRFIDNotFound
            };

            // Reference to the config
            var cfg = PLCA.LOADED_CONFIG;

            // Starts the request
            request.DoRequest(cfg.Host, cfg.Port, cfg.PrivateKey, rfid);

            // Executes if the user got logged in/out successfully
            void OnSuccess(bool status/*true=>login, false=>logout*/)
            {
                // Closes the overlay and clears the fields
                this.Dispatcher.Invoke(() => this.LoginResetForm());

                // Displays the short animation
                this.LoginStartDisplayCountdown(status);
            }

            // Executes if no user with the provided rfid could be found
            void OnRFIDNotFound()
            {
                // Displays the error
                this.DisplayInfo(
                    Lang.main_rfid_error_loading_title,
                    Lang.main_rfid_error_loading_text,
                    this.CloseOverlay,
                    Lang.main_popup_close
                );
            }
        });

        /// <summary>
        /// Displays an animation to indicate that the user has successfull been logged out or logged in
        /// </summary>
        /// <param name="loggedIn">If the user got logged in or out</param>
        private void LoginStartDisplayCountdown(bool loggedIn)
        {
            // Starts a new task to not block the main application
            Task.Run(() =>
            {
                // Iterates over the animation ten times with a delay of .2s
                for(int i = 0; i < 5; i++)
                {
                    // Displays the animation
                    this.DisplayInfo(loggedIn? Lang.main_login_rfid_success_title_login : Lang.main_login_rfid_success_title_logout, new string('.',i), null, null, false);
                    // Waits for a short while
                    Thread.Sleep(500);
                }

                // Closes the overlay
                this.CloseOverlay();
            });
        }

        /// <summary
        /// Displays the given user in the current login form
        /// </summary>
        /// <param name="user">The user that should be displayed</param>
        /// <param name="spenttime">The time span entity that should be used</param>
        private void LoginDisplayUser(SimpleUserEntity user, TimespentEntity spenttime)
        {
            // Sets the selected user and spenttime
            this.selectedLoginUser = user;

            // Updates all fields
            this.FieldFirstname.Text = user.Firstname;
            this.FieldLastname.Text = user.Lastname;
            this.FieldStart.Text = spenttime.Start.ToString();
            this.FieldEnd.Text = spenttime.Stop == default ? string.Empty : spenttime.Stop.ToString();

            // Checks if the user can login or logout
            if (spenttime.Stop == default)
                // Aktives the login button
                this.ButtonLogin.IsEnabled = true;
            else
            {
                // Shows the logout button and hides the login button
                this.ButtonLogout.Visibility = Visibility.Visible;
                this.ButtonLogin.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Resets the login form back to the default login screen
        /// </summary>
        private void LoginResetForm()
        {

            // Resets the buttons
            this.ButtonLogin.Visibility = Visibility.Visible;
            this.ButtonLogin.IsEnabled = false;
            this.ButtonLogout.Visibility = Visibility.Collapsed;

            // Resets all login fields
            foreach (TextBox field in this.loginFieldGroup)
                field.Text = string.Empty;

            // Unselects anything
            this.selectedLoginUser = null;
        }

        #endregion
    }
}

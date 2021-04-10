using Pl_Covid_19_Anmeldung;
using Pl_Covid_19_Anmeldung.connection.exceptions;
using Pl_Covid_19_Anmeldung.connection.requests;
using Pl_Covid_19_Anmeldung.utils;
using Pl_Covid_19_Anmeldung.windows.configWindow;
using Pl_Covid_19_Anmeldung.windows.requests;
using projektlabor.noah.planmeldung.Properties.langs;
using projektlabor.noah.planmeldung.utils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

/// <summary>
/// Different parts of the class have been outsources to other files inside "mainWindow/":
///     Login => MainWindowLogin.cs
///     Registration => MainWindowRegister.cs
///
/// All methods and variables in this outsources files will have something in their name that clarifies their origin.
/// 
/// Events from the form and external events will start with On<MethodName>
/// </summary>

namespace projektlabor.noah.planmeldung.windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Is used to select the serial port with the required esp32-rfid-scanner
        /// </summary>
        private RFIDReader rfidreader = new RFIDReader(".");

        /// <summary>
        /// Amount of times the logo has rotated. Used to animate an easter egg
        /// </summary>
        private int RotatedLogoTimes = 0;

        public MainWindow()
        {
            InitializeComponent();

            // Gets the login group
            this.loginFieldGroup = new TextBox[] { this.FieldStart, this.FieldEnd, this.FieldFirstname,this.FieldLastname };
        }

        #region Event-handlers

        #region Event-handlers Request-errors

        /// <summary>
        /// Handler if a request has an I/O error
        /// </summary>
        private void OnRequestErrorIO()
        {
            // Displays the io error
            this.DisplayInfo(
                Lang.main_request_error_io_title,
                Lang.main_request_error_io_info,
                this.CloseOverlay,
                Lang.main_request_error_button
            );
        }

        /// <summary>
        /// Handler if a request returned that the preset handler does not exist on the remote server.
        /// </summary>
        private void OnRequestErrorNonsense(NonsensicalError err)
        {
            // Gets the language key of the error
            //   Gets the attribute for the langauge as an enum property (Lang)
            //   Gets the value of the value from that enum property
            string langKey = err.GetAttribute<EnumProperty>(x => x.Key.Equals("lang")).GetValue<string>();

            // Gets the info
            string info = Lang.ResourceManager.GetString($"main.request.error.technical.{langKey}.text", Lang.Culture);

            // Displays the special error
            this.DisplayInfo(
                Lang.main_request_error_technical_title,
                Lang.main_request_error_technical_text + info,
                this.CloseOverlay,
                Lang.main_request_error_button
            );
        }

        #endregion

        /// <summary>
        /// Executes once the window loaded
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Loads the config
            Config.GetConfigFromUser((isNew,cfg,pw) =>
            {
                // Sets the config
                PLCA.LOADED_CONFIG = cfg;

                // Checks if the config got newly created
                if(isNew)
                    // Opens a new config-edit window
                    new ConfigWindow(cfg,pw).ShowDialog();
            }, this.Close);
        }

        /// <summary>
        /// Executes when an rfid gets recevied from the esp32-rfid-scanner
        /// </summary>
        /// <param name="rfid">The received rfid</param>
        private void OnRFIDReceiv(string rfid) => this.Dispatcher.Invoke(() =>
        {
            // If the overlay is visible
            bool over = this.Overlay.Visibility.Equals(Visibility.Visible);

            // Checks if the overlay is displayed
            if (over)
                return;

            // Checks if the register form handles the rfid
            if (this.FormRegister.OnRFIDReceive(rfid))
                return;

            // Start the login process using the rfid
            this.LoginUserFromRFID(rfid);
        });

        /// <summary>
        /// Executes when the button that should connect the rfid-scanner gets clicked
        /// </summary>
        private void OnRFIDButtonClicked(object sender, RoutedEventArgs e) => this.StartRFIDReader();

        /// <summary>
        /// Executes when the logo gets clicked
        /// </summary>
        private void OnLogoClick(object sender, RoutedEventArgs e)
        {
            // Updates the rotated times
            this.RotatedLogoTimes++;

            // Checks if the easter-easter-egg got activated
            if (this.RotatedLogoTimes >= 41)
            {
                // Resets the rotated times
                this.RotatedLogoTimes = 0;

                // Creates the transformer
                SkewTransform skew = new SkewTransform();
                // Starts the animation
                this.ButtonLogo.RenderTransform = skew;
                skew.BeginAnimation(
                    SkewTransform.AngleXProperty,
                    new DoubleAnimation(0,180, new Duration(TimeSpan.FromMilliseconds(2000)))
                );
            }
            else
            {
                // Creates the transformer
                RotateTransform rt = new RotateTransform();

                // Gets the rotate direction
                bool forward = this.RotatedLogoTimes % 2 == 0;

                // Gets the max amount of rotations
                int max = 360 + 2;

                // Starts the animation
                this.ButtonLogo.RenderTransform = rt;
                rt.BeginAnimation(
                    RotateTransform.AngleProperty,
                    new DoubleAnimation(forward ? max : 0, forward ? 0 : max, new Duration(TimeSpan.FromMilliseconds(200)))
                );
            }

        }

        /// <summary>
        /// Event to hook when a button should close the overly
        /// </summary>
        private void OnButtonCloseClicked(object sender, RoutedEventArgs e) => this.CloseOverlay();

        /// <summary>
        /// Event to open the config-edit window
        /// </summary>
        private void OnConfigButtonClicked(object sender, RoutedEventArgs e)
        {
            // Loads the config
            Config.GetConfigFromUser((_,cfg,pw) =>
            {
                // Opens a new config-edit window
                new ConfigWindow(cfg,pw).ShowDialog();
            },null);
        }

        #endregion

        #region Actions

        /// <summary>
        /// Used by seperated thread to start the background-rfid reader.
        /// Handles the successful connection to the rfid scanner and also the visuals.
        /// </summary>
        private void StartRFIDReader() => Task.Run(() =>
        {
            // Starts the rfid-reader
            this.rfidreader.Start(
                OnPortNotFound,
                OnPortFound,
                OnPortDisconnect,
                this.OnRFIDReceiv
            );

            // Executes if no port with a scanner could be found
            void OnPortNotFound()
            {
                this.DisplayInfo(
                    Lang.main_rfid_error_port_not_found_title,
                    Lang.main_rfid_error_port_not_found_text,
                    this.CloseOverlay,
                    Lang.main_popup_close
                );
            }

            // Executes once the port with a scanner got found
            void OnPortFound() => this.Dispatcher.Invoke(() =>
            {
                // Removes the overlay
                this.CloseOverlay();
                // Hides the error button for the disconnected rfid-scanner
                this.ButtonErrorRFID.Visibility = Visibility.Collapsed;
            });

            // Executes once the port gets disconnected again
            void OnPortDisconnect() => this.Dispatcher.Invoke(() =>
                this.ButtonErrorRFID.Visibility = Visibility.Visible
            );
        });
        
        /// <summary>
        /// Shows the given text with the title.
        /// </summary>
        /// <param name="title">The overlay title</param>
        /// <param name="text">The overlay text</param>
        /// <param name="buttonText">The text on the button</param>
        /// <param name="clickevent">The event handler that should execute when the user clicks on the button</param>
        /// <param name="errorColor">If the color should indicate an error</param>
        private void DisplayInfo(string title, string text, Action clickevent = null, string buttonText = null,bool errorColor = true) => this.Dispatcher.Invoke(() =>
        {
            // Shows the info and text
            this.OverlayInfoText.Text = text;
            this.OverlayInfoTitle.Text = title;
            this.OverlayInfoTitle.Foreground = new SolidColorBrush(errorColor?Colors.Red:Colors.Cyan);

            // Makes the info visible
            this.CloseOverlay();
            this.OverlayInfo.Visibility = this.Overlay.Visibility = Visibility.Visible;

            // Checks if the button should be useable
            bool displayButton = clickevent != null && buttonText != null;

            // Updats the button visiblity
            this.ButtonOverlayClose.Visibility = displayButton ? Visibility.Visible : Visibility.Collapsed;

            // Updates the button
            if (displayButton)
            {
                // Removes all previous events
                this.ButtonOverlayClose.RemoveRoutedEventHandlers(Button.ClickEvent);

                // Appends the new one
                this.ButtonOverlayClose.Click += (handler, args) => clickevent();

                // Appends the text
                this.ButtonOverlayClose.Content = buttonText;
            }
        });

        /// <summary>
        /// Shows the given error with the title
        /// </summary>
        /// <param name="text">The text that should be displayed at the title</param>
        private void DisplayLoading(string text) => this.Dispatcher.Invoke(() =>
        {
            // Shows the title
            this.OverlayLoadingTitle.Text = text;
            // Makes the error visible
            this.CloseOverlay();
            this.OverlayLoading.Visibility = this.Overlay.Visibility = Visibility.Visible;
        });

        /// <summary>
        /// Closes the overlay by hidding it
        /// </summary>
        private void CloseOverlay() => this.Dispatcher.Invoke(() =>
        {
            // Hides the global overlay
            this.Overlay.Visibility = Visibility.Collapsed;

            // Hides the single parts
            this.OverlayInfo.Visibility =
            this.OverlayLoading.Visibility = Visibility.Collapsed;
        });

        #endregion
    }
}

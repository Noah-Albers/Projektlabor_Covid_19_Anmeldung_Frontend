using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using projektlabor.noah.planmeldung.datahandling.entities;
using Pl_Covid_19_Anmeldung.connection.requests;
using Pl_Covid_19_Anmeldung;

namespace projektlabor.noah.planmeldung.windows.mainWindow
{
    /// <summary>
    /// Interaction logic for UserSearch.xaml
    /// </summary>
    public partial class UserSearch : UserControl
    {
        // Executer when an io error occurres
        public Action OnIOError { get; set; }
        // Executer when the server returns a known handler but one that does not make sense. Eg. a permission error where to applicatation can by default only request resources where the permission is given
        public Action<NonsensicalError> OnNonsenseError { get; set; }

        /// <summary>
        /// Event handler that executes when the a user got selected
        /// </summary>
        public Action<SimpleUserEntity> OnSelect { get; set; }
        
        /// <summary>
        /// Holds the current running task of fetching users from the database.
        /// Prevents the program form starting multiple tasks to do the same thing.
        /// </summary>
        private Task taskFetching;

        public UserSearch()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Executes when the button to search users gets clicked
        /// </summary>
        private void OnOpenButtonClick(object sender, RoutedEventArgs e)
        {
            // Resets the search bar
            this.FieldSearch.Text = string.Empty;

            // Shows the loading animation
            this.ViewLoading.Visibility = Visibility.Visible;

            // Hides the list view
            this.ViewList.Visibility = Visibility.Collapsed;

            // Checks if a request for all users is already running
            if (taskFetching != null)
                return;

            // Starts the task
            this.taskFetching = Task.Run(() =>
            {
                // Creates the request
                var req = new GrabUserRequest()
                {
                    OnNonsenseError = this.OnNonsenseError,
                    onReceive = this.OnReceiveUsers,
                    OnErrorIO = this.OnIOError
                };

                // Reference to the config
                var cfg = PLCA.LOADED_CONFIG;

                // Starts the request
                req.DoRequest(cfg.Host, cfg.Port, cfg.PrivateKey);

                // Resets the task
                this.taskFetching = null;
            });
        }
    
        /// <summary>
        /// Executes when the user selects another user
        /// </summary>
        private void OnSelectUser(object server, SelectionChangedEventArgs e)
        {
            // Checks that an item got selected
            if (this.List.SelectedItem == null)
                return;

            // Closes the popup
            this.Popup.IsOpen = false;

            // Gets the selected user
            // Updates the selected user
            SimpleUserEntity user = (this.List.SelectedItem as ListViewItem).Content as SimpleUserEntity;

            // Executes the handler
            this.OnSelect?.Invoke(user);
        }

        /// <summary>
        /// Executes when the user types to search
        /// </summary>
        private void OnSearch(object sender, TextChangedEventArgs e)
        {
            // Iterates over all listed users
            foreach (var userItem in this.List.Items)
            {
                // Gets the listitem
                var itm = userItem as ListViewItem;

                // Checks if the user matches
                // Shows or hides the user
                itm.Visibility = (itm.Content as SimpleUserEntity).IsMatching(this.FieldSearch.Text) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Executes when the users for get received
        /// </summary>
        /// <param name="users">The received users</param>
        private void OnReceiveUsers(SimpleUserEntity[] users) => this.Dispatcher.Invoke(() =>
        {
            // Removes all previous users
            this.List.Items.Clear();

            // Appends the new users
            foreach (var u in users)
            {
                // Creates the list item
                ListViewItem lvi = new ListViewItem
                {
                    // Creates the display
                    Content = u,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                // Appends the user
                this.List.Items.Add(lvi);
            }

            // Shows the user list
            this.ViewList.Visibility = Visibility.Visible;
            // Hides the loading animation
            this.ViewLoading.Visibility = Visibility.Collapsed;

            this.FieldSearch.Focus();
        });
    }
}

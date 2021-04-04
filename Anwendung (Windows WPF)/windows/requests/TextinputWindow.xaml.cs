using System;
using System.Windows;
using System.Windows.Controls;

namespace Pl_Covid_19_Anmeldung.windows.dialogs
{
    /// <summary>
    /// Interaction logic for TextinputWindow.xaml
    /// </summary>
    public partial class TextinputWindow : Window
    {
        // Handler for when a text gets submitted successfully
        public Action<string> OnSubmit;

        // Handler for when the form gets cancled
        public Action OnCancel;

        public TextinputWindow(string title,string displayTitle, Action<string> OnSubmit, Action OnCancel, string buttonSubmitText, string buttonCancleText)
        {
            InitializeComponent();

            this.Title = title;
            this.OnSubmit = OnSubmit;
            this.OnCancel = OnCancel;
            this.input.Title = displayTitle;
            this.btnOk.Content = buttonSubmitText;
            this.btnCancel.Content = buttonCancleText;
        }



        /// <summary>
        /// Handler for the button ok click event
        /// </summary>
        private void OnButtonOkClicked(object sender, RoutedEventArgs e)
        {
            // Removes the cancle event
            this.OnCancel = null;

            // Removes the close handler to prevent both from beeing called
            this.OnCancel = null;

            // Kills the window
            this.Close();

            // Executes the accept
            this.OnSubmit?.Invoke(this.input.Text);
        }

        /// <summary>
        /// Handler for when any text gets written on the input field
        /// </summary>
        /// <param name="_"></param>
        public void OnType(TextChangedEventArgs _) => this.btnOk.IsEnabled = this.input.Text.Length > 0;

        /// <summary>
        /// Handler for the button cancel event
        /// </summary>
        private void OnButtonCancelClicked(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Handler for the form-close event
        /// </summary>
        protected override void OnClosed(EventArgs e) => this.OnCancel?.Invoke();
    }
}

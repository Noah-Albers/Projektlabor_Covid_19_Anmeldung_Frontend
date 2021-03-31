using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pl_Covid_19_Anmeldung.windows.requestDialog
{
    /// <summary>
    /// Interaction logic for RequestDialog.xaml
    /// </summary>
    public partial class RequestDialog : Window
    {
        public string Text
        {
            get
            {
                return this.FieldInput.Title;
            }
            set
            {
                this.FieldInput.Title = value;
            }
        }
        public string TextOkButton
        {
            set
            {
                this.buttonOk.Content = value;
            }
        }
        public string TextCancleButton
        {
            set
            {
                this.buttonCancle.Content = value;
            }
        }

        // Handler for when a text gets submitted successfully
        public Action<string> OnSubmit;
        // Handler for when the form gets cancled
        public Action OnCancle;

        public RequestDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handler for the form-close event
        /// </summary>
        protected override void OnClosed(EventArgs e) => this.OnCancle?.Invoke();

        /// <summary>
        /// Handler for the button ok click event
        /// </summary>
        private void OnButtonOkClicked(object sender, RoutedEventArgs e)
        {
            // Removes the cancle event
            this.OnCancle = null;

            // Executes the accept
            this.OnSubmit?.Invoke(this.FieldInput.Text);

            // Kills the window
            this.Close();
        }

        /// <summary>
        /// Handler for the button cancle click event
        /// </summary>
        private void OnButtonCancleClicked(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Event handler for when the user types something
        /// </summary>
        private void OnType(TextChangedEventArgs _) =>
            // En/Disables the ok button based on if anything was written inside the fields
            this.buttonOk.IsEnabled = this.FieldInput.Text.Length > 0;
    }
}

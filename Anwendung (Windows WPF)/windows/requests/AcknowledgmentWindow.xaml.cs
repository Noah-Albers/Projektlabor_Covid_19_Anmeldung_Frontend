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

namespace Pl_Covid_19_Anmeldung.windows.requests
{
    /// <summary>
    /// Interaction logic for AcknowledgmentWindow.xaml
    /// </summary>
    public partial class AcknowledgmentWindow : Window
    {
        // Callback when the form gets closed
        private readonly Action OnAcknowledge;

        public AcknowledgmentWindow(string title,string display, Action OnAcknowledge, string buttonAcknowledgeText)
        {
            InitializeComponent();
            this.Title = title;
            this.OnAcknowledge = OnAcknowledge;
            this.title.Text = display;
            this.button.Content = buttonAcknowledgeText;
        }

        /// <summary>
        /// Handler that executes when the window closes
        /// </summary>
        protected override void OnClosed(EventArgs e) => this.OnAcknowledge?.Invoke();



        /// <summary>
        /// Handler when to button gets clicked
        /// </summary>
        private void OnButtonClicked(object sender, RoutedEventArgs e) => this.Close();
    }
}

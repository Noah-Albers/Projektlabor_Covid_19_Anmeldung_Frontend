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
    /// Interaction logic for YesNoWindow.xaml
    /// </summary>
    public partial class YesNoWindow : Window
    {
        // Handlers for the different events
        private Action OnNo;
        private readonly Action OnExtra, OnYes;

        public YesNoWindow(string title,Action OnYes,Action OnNo,string yesText,string noText,string titleText,string informationText,Action OnExtra = null,string extraText=null)
        {
            InitializeComponent();
            this.Title = title;
            this.OnYes = OnYes;
            this.OnNo = OnNo;
            this.OnExtra = OnExtra;
            this.btnExtra.Content = extraText;
            this.btnYes.Content = yesText;
            this.btnNo.Content = noText;
            this.title.Text = titleText;
            this.infoText.Text = informationText;

            this.btnExtra.Visibility = OnExtra == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Handler for the extra button
        /// </summary>
        private void OnButtonExtra(object sender, RoutedEventArgs e) => this.OnExtra?.Invoke();

        /// <summary>
        /// Handler for the yes button
        /// </summary>
        private void OnButtonYes(object sender, RoutedEventArgs e)
        {
            this.OnNo = null;
            this.Close();
            this.OnYes?.Invoke();
        }

        /// <summary>
        /// Handler for the no button
        /// </summary>
        private void OnButtonNo(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Handler for when the form closes
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e) => this.OnNo?.Invoke();
    }
}

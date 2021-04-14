using Pl_Covid_19_Anmeldung;
using System;
using System.Windows;

namespace projektlabor.noah.planmeldung
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Starts the logger
            try
            {
                Logger.init(
                    "logs/",
                    Logger.DEBUG | Logger.INFO | Logger.WARNING | Logger.ERROR,
                    Logger.INFO | Logger.WARNING | Logger.ERROR
                );
            }
            catch (Exception e)
            {
                // Displays the info
                MessageBox.Show("Failed to start logger: " + e.Message);
                // Kills the app
                Current.Shutdown(-1);
            }
        }
    }
}

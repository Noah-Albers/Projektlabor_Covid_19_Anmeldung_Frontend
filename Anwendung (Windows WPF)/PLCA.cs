using projektlabor.noah.planmeldung;

namespace Pl_Covid_19_Anmeldung
{
    class PLCA
    {
        /// <summary>
        /// Path to the config file
        /// </summary>
        public static readonly string CONFIG_PATH = "Config.bin";

        /// <summary>
        /// The loaded config of the program
        /// </summary>
        public static Config LOADED_CONFIG;

        /// <summary>
        /// The logger for the program
        /// </summary>
        public static readonly Logger LOGGER = new Logger(Logger.ALL);
    }
}

using Pl_Covid_19_Anmeldung;
using System;
using System.IO.Ports;

namespace projektlabor.noah.planmeldung.utils
{
    class RFIDReader
    {
        // Reference to the logger of the program
        private readonly Logger log = new Logger("RFID-Reader");

        /// <summary>
        /// Holds the serial connection
        /// </summary>
        private SerialPort con;

        /// <summary>
        /// The char that will be send to end a message
        /// </summary>
        private string endIndicator;

        public RFIDReader(string endIndicator)
        {
            this.endIndicator = endIndicator;
        }

        /// <summary>
        /// Searches the esp32-rfid-scanner on every usb-com port. It executes the coresponding event for every action specified.
        /// If the scanner got found, it start the listen for incoming data.
        /// </summary>
        public void Start(Action onNoPortFound, Action onPortFound, Action onPortDisconnect, Action<string> onReceivId)
        {
            // Gets all ports
            string[] ports = SerialPort.GetPortNames();

            this.log
                .Info("Starting to request all ports")
                .Critical($"Ports=[{string.Join(",",ports)}]");

            // Checks all ports if the esp32-rfid-scanner is connected
            foreach (string p in ports)
            {
                try
                {
                    // Creates the connection
                    SerialPort sp = new SerialPort(p, 9600);
                    // Opens it
                    sp.Open();
                    // Sends the auth request character
                    sp.Write("I");
                    // Gets the starting wait time
                    long millis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    // Will hold all data send by the device
                    string data = string.Empty;

                    // Waits 100 ms for the port to confirm that the port is from the esp32-rfid-scanner
                    while (DateTimeOffset.Now.ToUnixTimeMilliseconds() - millis < 100)
                    {
                        // Appends all new data
                        data += sp.ReadExisting();

                        // Checks if the data matches
                        if (data.Equals("RFIDEspScanner"))
                        {
                            // Updates the connection
                            this.con = sp;

                            // Executes the success event
                            onPortFound();

                            log.Info("RFID-Scanner found on port: "+p);

                            // Handle the reading of the serial id's
                            this.HandleReading(onReceivId, onPortDisconnect);
                            return;
                        }
                    }
                }
                catch { }
            }

            this.log.Info("No RFID-Scanner could be found");

            // Execute without finding a port
            onNoPortFound();
        }

        /// <summary>
        /// Executes once a valid com-port for the esp32-rfid-scanner got found
        /// </summary>
        /// <param name="onReceivId">The event handler that triggers when an id gets received</param>
        /// <param name="onDisconnect">The event handler that trigger when the port gets disconnected</param>
        private void HandleReading(Action<string> onReceivId, Action onDisconnect)
        {
            try
            {
                // Waits for eternity for new codes (Or until the programm gets closed or the port gets disconnected)
                while (true)
                {
                    // Holds all data until a new endIndicator gets received
                    string data = string.Empty;

                    // Reads all data
                    while (!data.EndsWith(this.endIndicator))
                        data += (char)con.ReadChar();

                    // Gets the id
                    string id = data.Substring(0, data.Length - this.endIndicator.Length);

                    this.log
                        .Debug("Received rfid")
                        .Critical(id);

                    // Executes the id event
                    onReceivId(id);
                }
            }
            catch
            {
                // Executes the error handler
                onDisconnect();
            }
        }

    }
}

using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.connection.exceptions;
using Pl_Covid_19_Anmeldung.utils;
using projektlabor.noah.planmeldung.Properties.langs;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    abstract class PLCARequest
    {
        // Random generator
        private readonly static Random RDM_GENERATOR = new Random();

        // Reference to the logger for the program
        protected static Logger log;

        // Executer when an io error occurres
        public Action OnErrorIO;
        // Executer when the server returns a known handler but one that does not make sense. Eg. a permission error where to applicatation can by default only request resources where the permission is given
        public Action<NonsensicalError> OnNonsenseError;

        /// <summary>
        /// Generates a logger with a random id that can be 
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        protected Logger GenerateLogger(string presetName) => new Logger(presetName + "." + RDM_GENERATOR.Next());

        /// <summary>
        /// The endpoint id at the server. Can be seen like a path in http
        /// </summary>
        protected abstract int GetEndpointId();

        /// <summary>
        /// Handles any fatal errors if any occure.
        /// Fatal errors are errors that occurre before the handler for the request is executed on the server.
        /// Usually those occurre when a request has no valid endpoint or data for the handler.
        /// </summary>
        /// <param name="exc">The except-error-string to determin what kind of except-error occurred</param>
        /// <param name="log">The logger to log the error to</param>
        /// <returns>The json-object that can be forwarded as a response from the server. Otherwise just throw an error</returns>
        /// <exception cref="Exception">Can throw an exception if no special handling for the error is required</exception>
        protected void HandleFatalError(string exc,Logger log)
        {
            log.Debug("Fatal error returned by remote server:"+exc);

            // Checks the returned error
            switch (exc)
            {
                case "auth":
                    this.OnNonsenseError?.Invoke(NonsensicalError.AUTH_SERVER);
                    break;
                case "handler":
                    this.OnNonsenseError?.Invoke(NonsensicalError.HANDLER);
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;
            }
        }

        /// <summary>
        /// Does the usual request with simple error handling (Callbacks).
        /// Executes the onReceive function if a valid response got returned that can be handled.
        /// </summary>
        /// <param name="host">Ip/Domain to send the request to</param>
        /// <param name="port">Port on which the server is running</param>
        /// <param name="privateKey">The current provided private key</param>
        /// <param name="requestData">The request object that defines any special data that might be required to fullfil the request.</param>
        /// <param name="onReceive">The callback to handle data if it got received successfully. The callback can throw an error. It will be catched and the unknown error callback will be executed</param>
        /// <param name="onError">The callback to handle any error that might have been returned by the remote handler. Exceptions that will be thrown will be handled as unknown error, same as the receive.</param>
        protected void DoRequest(Logger log,string host, int port, RSAParameters privateKey, JObject requestData,Action<JObject> onReceive,Action<string,JObject> onError = null)
        {
            try
            {
                // Starts the connection
                using (PLCASocket socket = new PLCASocket(log,host, port, privateKey))
                {
                    // Creates the request
                    JObject request = new JObject()
                    {
                        ["endpoint"] = this.GetEndpointId(),    // Appends the endpoint
                        ["data"] = requestData                  // Appends the request-data
                    };

                    // Gets the bytes
                    byte[] requestBytes = Encoding.UTF8.GetBytes(request.ToString());

                    log
                        .Debug("Sending request...")
                        .Critical($"Bytes=[{string.Join(",", requestBytes)}]");

                    // Sends the request
                    socket.SendPacket(requestBytes);

                    log.Debug("Waiting for response");

                    // Waits for the response
                    JObject resp = JObject.Parse(Encoding.UTF8.GetString(socket.ReceivePacket()));

                    // Checks if an except occurred
                    if (resp.ContainsKey("except"))
                    {
                        // Handles the fatal error
                        this.HandleFatalError((string)resp["except"],log);
                        return;
                    }
                    
                    // Checks the status
                    if ((int)resp["status"] != 1)
                    {
                        // Checks that the errorcode got passed
                        if (!resp.ContainsKey("errorcode"))
                            throw new Exception("Received invalid response");

                        // Handles the error (data is nullable)
                        onError?.Invoke((string)resp["errorcode"], (JObject)resp["data"]);
                        return;
                    }

                    // Checks that the data got passed
                    if (!resp.ContainsKey("data"))
                        throw new Exception("Received invalid response");

                    // Gets the actual response from the handler
                    onReceive?.Invoke(
                        (JObject)resp["data"]
                    );
                }
            }
            catch (HandshakeException)
            {
                this.OnNonsenseError?.Invoke(NonsensicalError.AUTH_KEY);
            }
            catch (Exception e)
            {
                // Checks if the error is an io-error
                if (e is IOException || e is SocketException)
                    // Handle the io-error
                    this.OnErrorIO?.Invoke();
                else
                {
                    log
                        .Debug("Unknown error occurred while performing the request")
                        .Critical(e.Message);

                    // Unknown error
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                }
            }
        }
    }

    /// <EnumProperty>lang - the name of the enum value that can be used to grab a certaint information from the Language file.</EnumProperty>
    public enum NonsensicalError
    {   
        [EnumProperty("lang","authserver")]
        AUTH_SERVER,
        [EnumProperty("lang","handler")]
        HANDLER,
        [EnumProperty("lang","authkey")]
        AUTH_KEY,
        [EnumProperty("lang","database")]
        SERVER_DATABASE,
        [EnumProperty("lang","unknown")]
        UNKNOWN,
        [EnumProperty("lang","lessreturn")]
        LESS_DATA
    }
}

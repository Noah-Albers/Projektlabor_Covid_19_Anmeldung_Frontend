using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.connection.exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    abstract class PLCARequest
    {
        // Reference to the logger for the program
        protected static Logger log = PLCA.LOGGER;

        // Executer when an handshake error occurres
        public Action<HandshakeExceptionType> onErrorHandshake;
        // Executer when an io error occurres
        public Action onErrorIO;
        // Executer when an unknow error occurres
        public Action onUnknownError;
        // Executer when a auth error occurres
        public Action onAuthError;
        // Executer when the server returns that the handler wasn't found (May server and client have a different version?)
        public Action onHandlerError;

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
        /// <returns>The json-object that can be forwarded as a response from the server. Otherwise just throw an error</returns>
        /// <exception cref="Exception">Can throw an exception if no special handling for the error is required</exception>
        protected void HandleFatalError(string exc)
        {
            log.Debug("Fatal error returned by remote server: "+exc);

            // Checks the returned error
            switch (exc)
            {
                case "auth":
                    this.onAuthError?.Invoke();
                    break;
                case "handler":
                    this.onHandlerError?.Invoke();
                    break;
                default:
                    this.onUnknownError?.Invoke();
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
        protected void DoRequest(string host, int port, RSAParameters privateKey, JObject requestData,Action<JObject> onReceive,Action<string,JObject> onError = null)
        {
            try
            {
                // Starts the connection
                using (PLCASocket socket = new PLCASocket(host, port, privateKey))
                {
                    // Creates the request
                    JObject request = new JObject()
                    {
                        ["endpoint"] = this.GetEndpointId(),    // Appends the endpoint
                        ["data"] = requestData                  // Appends the request-data
                    };

                    log.Debug("Sending request...");

                    // Sends the request
                    socket.SendPacket(Encoding.UTF8.GetBytes(request.ToString()));

                    log.Debug("Waiting for response");

                    // Waits for the response
                    JObject resp = JObject.Parse(Encoding.UTF8.GetString(socket.ReceivePacket()));

                    // Checks if an except occurred
                    if (resp.ContainsKey("except"))
                    {
                        // Handles the fatal error
                        this.HandleFatalError((string)resp["except"]);
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
            catch (HandshakeException e)
            {
                this.onErrorHandshake?.Invoke(e.Type);
            }
            catch (Exception e)
            {
                // Checks if the error is an io-error
                if (e is IOException || e is SocketException)
                    // Handle the io-error
                    this.onErrorIO?.Invoke();
                else
                {
                    log.Debug("Unknown error occurred while performing the request");
                    log.Critical(e.Message);

                    // Unknown error
                    this.onUnknownError?.Invoke();
                }
            }
        }
    }
}

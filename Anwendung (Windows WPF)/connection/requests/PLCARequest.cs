using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.connection.exceptions;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    abstract class PLCARequest
    {
        // Executer when an handshake error occurres
        public Action<HandshakeExceptionType> onErrorHandshake;
        // Executer when an io error occurres
        public Action onErrorIO;
        // Executer when an unknow error occurres
        public Action onUnknownError;

        /// <summary>
        /// The endpoint id at the server. Can be seen like a path in http
        /// </summary>
        protected abstract int GetEndpointId();

        /// <summary>
        /// Handles any fatal errors if any occure.
        /// Fatal errors are errors that occurre before the handler for the request is executed on the server. Usually those occurre when a request has no valid endpoint or data for the handler.
        /// </summary>
        /// <param name="exc">The except-error-string to determin what kind of except-error occurred</param>
        /// <returns>The json-object that can be forwarded as a response from the server. Otherwise just throw an error</returns>
        /// <exception cref="Exception">Can throw an exception if no special handling for the error is required</exception>
        protected JObject HandleFatalError(string exc)
        {
            throw new IOException();
        }

        /// <summary>
        /// Does the usual request with simple error handling (Callbacks).
        /// Executes the onReceive function if a valid response got returned that can be handled.
        /// </summary>
        /// <param name="host">Ip/Domain to send the request to</param>
        /// <param name="port">Port on which the server is running</param>
        /// <param name="privateKey">The current provided private key</param>
        /// <param name="requestData">The request object that defines any special data that might be required to fullfil the request.</param>
        /// <param name="onReceive">The callback to handle data if it got received successfully</param>
        protected void DoRequest(string host, int port, RSAParameters privateKey, JObject requestData,Action<JObject> onReceive)
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

                    // Sends the request
                    socket.SendPacket(Encoding.UTF8.GetBytes(request.ToString()));

                    // Waits for the response
                    JObject resp = JObject.Parse(Encoding.UTF8.GetString(socket.ReceivePacket()));

                    onReceive(
                        // Checks for a fatal exception
                        resp.ContainsKey("except") ?
                        // Handles the error
                        this.HandleFatalError((string)resp["except"]) :
                        // Gets the actual response from the handler
                        (JObject)resp["data"]
                    );
                }
            }
            catch (HandshakeException e)
            {
                // TODO: Log
                this.onErrorHandshake?.Invoke(e.Type);
            }
            catch (Exception e)
            {
                // TOOD: Log
                // Checks if the error is an io-error
                if (e is IOException || e is SocketException)
                    // Handle the io-error
                    this.onErrorIO?.Invoke();
                else
                    // Unknown error
                    this.onUnknownError?.Invoke();
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}

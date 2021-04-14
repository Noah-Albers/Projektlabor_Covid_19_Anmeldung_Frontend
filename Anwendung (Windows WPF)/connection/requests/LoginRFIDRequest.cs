using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class LoginRFIDRequest : PLCARequest
    {
        // If the user hasn't been found (Using that rfid)
        public Action OnUserNotFound;

        // If the user's login/logout was successfull (Bool shows if the user got logged in (true) or logged out (false))
        public Action<bool/*Login(true) Logout(false)*/> OnSuccess;

        protected override int GetEndpointId() => 5;

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of a user</param>
        public void DoRequest(string host, int port, RSAParameters rsa, string rfid)
        {
            // Generates the logger
            Logger log = this.GenerateLogger("LoginRFIDRequest");

            log
                .Debug("Starting request to login/logout user (Using RFID)")
                .Critical("RFID=" + rfid);

            // Starts the request
            this.DoRequest(log,host, port, rsa, new JObject()
            {
                ["rfid"] = rfid
            }, res =>
            {
                // Gets the status
                bool status = (bool)res["status"]; 

                log
                    .Debug("Request was successfull")
                    .Critical("User is not logged " + (status ? "in" : "out"));

                this.OnSuccess?.Invoke(status);
            }, (a,b)=>this.OnFailure(log,a,b));
        }

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(Logger log,string err, JObject resp)
        {
            log
                .Debug("Failed to login user using rfid: "+err)
                .Critical(resp);

            switch (err)
            {
                // Server error (Database error eg. unreachable)
                case "database":
                    this.OnNonsenseError?.Invoke(NonsensicalError.SERVER_DATABASE);
                    break;
                // User not found (using rfid)
                case "user":
                    this.OnUserNotFound?.Invoke();
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;
            }
        }

    }
}

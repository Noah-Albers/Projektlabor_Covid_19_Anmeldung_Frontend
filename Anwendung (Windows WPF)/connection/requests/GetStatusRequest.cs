using Newtonsoft.Json.Linq;
using projektlabor.noah.planmeldung.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class GetStatusRequest : PLCARequest
    {
        public Action onServerError;

        // If the server returns that the send id is invalid
        public Action onUserNotFound;

        // If the user is not logged in
        public Action onUserNotLoggedIn;

        // If the user is logged in
        public Action<TimespentEntity> onUserLoggedIn;

        protected override int GetEndpointId()
        {
            return 1;
        }

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of a user</param>
        public void DoRequest(string host, int port, RSAParameters rsa, int userId) =>
            this.DoRequest(host, port, rsa, new JObject()
            {
                ["id"] = userId
            }, this.OnSuccess,this.OnFailure);
        
        /// <summary>
        /// Handler for a successfull request
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnSuccess(JObject resp)
        {
            // If the user is logged in
            bool loggedIn = (bool)resp["loggedin"];

            // Checks if the user isn't logged in
            if (!loggedIn)
                // Executes the not logged in callback
                this.onUserNotLoggedIn?.Invoke();
            else
            {
                // Creates the timespent entity
                var ts = new TimespentEntity()
                {
                    Start = (DateTime)resp["start"]
                };

                // Executes the logged in callback with an unfinished timespent entity
                this.onUserLoggedIn?.Invoke(ts);
            }
        }

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(string err,JObject resp)
        {
            // Checks if the error is the userid
            if (err.Equals("user"))
                // Executes the unknow user id
                this.onUserNotFound?.Invoke();
            // Checks for a server error
            else if (err.Equals("database"))
                // Executes the server error
                this.onServerError?.Invoke();
            else
                // Other erros make little sens as all required parameters have been provided
                this.onUnknownError?.Invoke();
        }

    }
}

using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class LoginRequest : PLCARequest
    {
        // If the server has an error (Database not reachable or smth)
        public Action onServerError;
        // If the user is already logged in
        public Action onUnauthorizedError;
        // If the user hasn't been found
        public Action onUserNotFound;

        // If the user's login was successfull
        public Action onSuccessfullLogin;

        protected override int GetEndpointId()
        {
            return 2;
        }

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of a user</param>
        public void DoRequest(string host, int port, RSAParameters rsa, int userId) =>
            this.DoRequest(host, port, rsa, new JObject() {
                ["id"] = userId
            }, _ => this.onSuccessfullLogin?.Invoke(), this.OnFailure);

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(string err, JObject resp)
        {
            switch (err)
            {
                // Server error (Database error eg. unreachable)
                case "database":
                    this.onServerError?.Invoke();
                    break;
                // User not found
                case "user":
                    this.onUserNotFound?.Invoke();
                    break;
                // User is already logged in (Log out first)
                case "unauthorized":
                    this.onUnauthorizedError?.Invoke();
                    break;
                default:
                    this.onUnknownError?.Invoke();
                    break;
            }
        }

    }
}

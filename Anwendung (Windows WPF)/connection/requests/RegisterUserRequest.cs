using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.datahandling.exceptions;
using projektlabor.noah.planmeldung.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class RegisterUserRequest : PLCARequest
    {
        // If the server has an error (Database not reachable or smth)
        public Action onServerError;
        // Server returned, that we have seen an invlid user
        public Action onInvalidUserServer;
        // If eigther an unexpected user input got found or an unknown exception occurred
        public Action<string> onInvalidInputOrUnknownError;
        // If a required value is missing
        public Action<string> onMissingValue;
        // If a user with the name and lastname already exists
        public Action onUserAlredyExists;

        // If the registration was successfull (Holds the user's id)
        public Action<int> onSuccess;

        // All entry's that the user has to pass
        private static readonly string[] REGISTER_ENTRYS = {
		    UserEntity.AUTODELETE,
		    UserEntity.HOUSE_NUMBER,
		    UserEntity.LOCATION,
		    UserEntity.POSTAL_CODE,
		    UserEntity.STREET,
		    UserEntity.FIRSTNAME,
		    UserEntity.LASTNAME
        };

        // All optional entry's that the user can pass
        private static readonly string[] OPTIONAL_REGISTER_ENTRYS = {
		    UserEntity.EMAIL,
		    UserEntity.TELEPHONE,
		    UserEntity.RFID
        };

        protected override int GetEndpointId()
        {
            return 4;
        }

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of the user that shall be loged out</param>
        public void DoRequest(string host, int port, RSAParameters rsa, UserEntity user)
        {
            log.Debug("Starting request to register a new user.");
            log.Critical(user.ToString());

            // The object that will hold the request
            JObject request = new JObject();

            try
            {
                // Tries to save the user
                user.Save(request, REGISTER_ENTRYS, OPTIONAL_REGISTER_ENTRYS);
            }catch(RequiredEntitySerializeException e)
            {
                log.Warn("Failed to serialize entity. Missing value.");
                log.Critical("Missing=" + e.KeyName);

                this.onMissingValue?.Invoke(e.KeyName);
                return;
            }
            catch(EntitySerializeException e)
            {
                log.Warn("Invalid input or unknown error while serializing entity.");
                log.Critical("Parameter=" + e.KeyName);

                this.onInvalidInputOrUnknownError?.Invoke(e.KeyName);
                return;
            }

            // Starts the request
            this.DoRequest(host, port, rsa, request, resp=>
            {
                // Gets the id
                int id = (int)resp["id"];

                log.Debug("Successfully registered a new user");
                log.Critical("New userid: " + id);

                // Executes the success handler with the received id
                this.onSuccess?.Invoke(id);
            }, this.OnFailure);
        }

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(string err,JObject resp)
        {
            log.Debug("Failed to register user: "+err);
            log.Critical(resp);

            // Checks the error
            switch (err)
            {
                case "database":
                    this.onServerError?.Invoke();
                    break;
                case "user":
                    this.onInvalidUserServer?.Invoke();
                    break;
                case "duplicated":
                    this.onUserAlredyExists?.Invoke();
                    break;
                default:
                    this.onUnknownError?.Invoke();
                    break;

            }
        }

    }
}

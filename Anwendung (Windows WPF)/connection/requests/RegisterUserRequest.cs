using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.datahandling.exceptions;
using projektlabor.noah.planmeldung.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class RegisterUserRequest : PLCARequest
    {
        // Server returned, that we have seen an invlid user
        public Action OnInvalidUserServer;
        // If a required value is missing
        public Action<string> OnMissingValue;
        // If a user with the name and lastname already exists
        public Action OnUserAlredyExists;

        // If the registration was successfull (Holds the user's id)
        public Action<int> OnSuccess;

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

        protected override int GetEndpointId() => 4;

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
                log.Warn("Failed to serialize entity. Missing value. Missing = " + e.KeyName);

                this.OnNonsenseError?.Invoke(NonsensicalError.LESS_DATA);
                return;
            }
            catch(EntitySerializeException e)
            {
                log.Warn("Invalid input or unknown error while serializing entity. Parameter="+ e.KeyName);

                this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
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
                this.OnSuccess?.Invoke(id);
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
                    this.OnNonsenseError?.Invoke(NonsensicalError.SERVER_DATABASE);
                    break;
                case "user":
                    this.OnInvalidUserServer?.Invoke();
                    break;
                case "duplicated":
                    this.OnUserAlredyExists?.Invoke();
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;

            }
        }

    }
}

using Newtonsoft.Json.Linq;
using projektlabor.noah.planmeldung.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class GrabUserRequest : PLCARequest
    {
        // Executer when the request has success
        public Action<SimpleUserEntity[]> onReceive;

        protected override int GetEndpointId()
        {
            return 0;
        }

        /// <summary>
        /// Starts the request
        /// </summary>
        public void DoRequest(string host, int port, RSAParameters rsa) =>
        this.DoRequest(host, port, rsa, new JObject(), resp =>
        {

            // Checks if the users are given
            if (!resp.ContainsKey("users"))
            {
                this.onUnknownError();
                return;
            }

            // Gets the users
            object rawUsers = resp["users"];

            // Checks if the users are given as an array
            if(!(rawUsers is JArray))
            {
                this.onUnknownError();
                return;
            }

            // Gets the raw users as an jarray
            JArray rawUserArr = ((JArray)rawUsers);

            // Creates the array
            SimpleUserEntity[] users = new SimpleUserEntity[rawUserArr.Count];

            // Parses every user
            for(int i = 0; i < users.Length; i++)
                try
                {
                    // Tries to load the user
                    var usr = new SimpleUserEntity();
                    usr.Load((JObject)rawUserArr[i], SimpleUserEntity.ENTRYS_LIST);
                }
                catch
                {
                    // If any user failes to load, the request is invalid
                    this.onUnknownError();
                    return;
                }

            // Sends all received users
            this.onReceive?.Invoke(users);
        });
    }
}

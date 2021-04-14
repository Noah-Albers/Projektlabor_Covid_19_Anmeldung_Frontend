using Newtonsoft.Json.Linq;
using projektlabor.noah.planmeldung.datahandling.entities;
using System;
using System.Security.Cryptography;
using System.Xml;

namespace Pl_Covid_19_Anmeldung.connection.requests
{
    class GrabUserRequest : PLCARequest
    {
        // Executer when the request has success
        public Action<SimpleUserEntity[]> onReceive;

        protected override int GetEndpointId() => 0;

        /// <summary>
        /// Starts the request
        /// </summary>
        public void DoRequest(string host, int port, RSAParameters privateKey)
        {
            // Gets the logger
            Logger log = this.GenerateLogger("GrabUserRequest");

            log.Debug("Starting request to fetch all users (Simple version)");

            // Starts the request
            this.DoRequest(
                log,
                host,
                port,
                privateKey,
                new JObject(),
                x=>this.OnReceive(x,log), (_, _2) => this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN)
            );
        }

        private void OnReceive(JObject resp,Logger log)
        {

            // Gets the raw users as an jarray
            JArray rawUserArr = (JArray)resp["users"];

            log
                .Debug("Received users")
                .Critical($"Users={rawUserArr.ToString(Newtonsoft.Json.Formatting.None)}");

            // Creates the array
            SimpleUserEntity[] users = new SimpleUserEntity[rawUserArr.Count];

            // Parses every user
            for (int i = 0; i < users.Length; i++)
                try
                {
                    // Tries to load the user
                    users[i] = new SimpleUserEntity();
                    users[i].Load((JObject)rawUserArr[i], SimpleUserEntity.ENTRYS_LIST);
                }
                catch
                {
                    // If any user failes to load, the request is invalid
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    return;
                }

            // Sends all received users
            this.onReceive?.Invoke(users);
        }
    }
}

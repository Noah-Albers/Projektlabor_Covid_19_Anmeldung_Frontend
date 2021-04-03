﻿using Newtonsoft.Json.Linq;
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
        public void DoRequest(string host, int port, RSAParameters rsa)
        {
            log.Debug("Starting request to fetch all users (Simple version)");

            // Starts the request
            this.DoRequest(host, port, rsa, new JObject(), OnReceive, (_, _2) => this.onUnknownError?.Invoke());
        }

        private void OnReceive(JObject resp)
        {

            // Gets the raw users as an jarray
            JArray rawUserArr = (JArray)resp["users"];

            log.Debug("Received users");
            log.Critical(rawUserArr.ToString());

            // Creates the array
            SimpleUserEntity[] users = new SimpleUserEntity[rawUserArr.Count];

            // Parses every user
            for (int i = 0; i < users.Length; i++)
                try
                {
                    // Tries to load the user
                    var usr = new SimpleUserEntity();
                    usr.Load((JObject)rawUserArr[i], SimpleUserEntity.ENTRYS_LIST);
                }
                catch
                {
                    // If any user failes to load, the request is invalid
                    this.onUnknownError?.Invoke();
                    return;
                }

            // Sends all received users
            this.onReceive?.Invoke(users);
        }
    }
}

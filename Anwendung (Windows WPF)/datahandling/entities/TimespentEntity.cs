using Pl_Covid_19_Anmeldung.datahandling;
using Pl_Covid_19_Anmeldung.datahandling.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace projektlabor.noah.planmeldung.datahandling.entities
{
    class TimespentEntity : Entity
    {
        public const string
        ID = "id",
        START = "start",
        STOP = "stop",
        DISCONNECTED_ON_END = "enddisconnect",
        USER_ID = "userid";

        // Holds all entrys
        private static readonly Dictionary<string, FieldInfo> ENTRYS = GetEntrys(typeof(TimespentEntity));

        // Holds a list with all names for the entrys
        public static readonly string[] ENTRYS_LIST = ENTRYS.Select(i => i.Key).ToArray();

        /// The unique id of the timespent entity
        [EntityInfo(ID)]
        public int? Id;

        /// When the user started to work
        [EntityInfo(START)]
        public DateTime? Start;

        /// When the user ended his work
        [EntityInfo(STOP)]
        public DateTime? Stop;

        /// If the day-end stopped the work
        [EntityInfo(DISCONNECTED_ON_END)]
        public bool? Enddisconnect;

        /// The user's id this timespententity belongs to
        [EntityInfo(USER_ID)]
        public int? UserId;

        protected override Dictionary<string, FieldInfo> Entrys()
        {
            return ENTRYS;
        }
    }
}

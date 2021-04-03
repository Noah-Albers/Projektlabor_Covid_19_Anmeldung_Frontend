using Pl_Covid_19_Anmeldung.datahandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace projektlabor.noah.planmeldung.datahandling.entities
{
    public class UserEntity : SimpleUserEntity
    {
        public const string
        POSTAL_CODE = "postalcode",
	    LOCATION = "location",
	    STREET = "street",
	    HOUSE_NUMBER = "housenumber",
	    TELEPHONE = "telephone",
	    EMAIL = "email",
	    RFID = "rfidcode",
	    AUTODELETE = "autodeleteaccount",
	    REGISTER_DATE = "createdate";

        // Holds all entrys
        private static readonly Dictionary<string, FieldInfo> ENTRYS = GetEntrys(typeof(UserEntity));

        // Holds a list with all names for the entrys
        public static readonly new string[] ENTRYS_LIST = ENTRYS.Select(i => i.Key).ToArray();

        [EntityInfo(LOCATION)]
        public string Location;
        [EntityInfo(POSTAL_CODE)]
        public int? PLZ;
        [EntityInfo(STREET)]
        public string Street;
        [EntityInfo(HOUSE_NUMBER)]
        public string StreetNumber;
        [EntityInfo(TELEPHONE)]
        public string TelephoneNumber;
        [EntityInfo(EMAIL)]
        public string Email;
        [EntityInfo(RFID)]
        public string Rfid;
        [EntityInfo(AUTODELETE)]
        public bool? AutoDeleteAccount;
        [EntityInfo(REGISTER_DATE)]
        public DateTime RegisterDate;

        protected override Dictionary<string, FieldInfo> Entrys()
        {
            return ENTRYS;
        }

        public override string ToString()
        {
            return $"{base.ToString()} Loc= {this.Street} {this.StreetNumber} {this.Location} {this.PLZ} Telephone={this.TelephoneNumber} Email={this.Email} RFID={this.Rfid} Autodelete={this.AutoDeleteAccount} Registered={this.RegisterDate}";
        }
    }
}

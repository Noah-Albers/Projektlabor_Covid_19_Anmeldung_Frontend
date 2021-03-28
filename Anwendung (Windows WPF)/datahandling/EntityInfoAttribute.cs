using System;

namespace Pl_Covid_19_Anmeldung.datahandling
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    class EntityInfoAttribute : Attribute
    {
        public string JsonName { get; private set; }


        public EntityInfoAttribute(string jsonName)
        {
            this.JsonName = jsonName;
        }

    }
}

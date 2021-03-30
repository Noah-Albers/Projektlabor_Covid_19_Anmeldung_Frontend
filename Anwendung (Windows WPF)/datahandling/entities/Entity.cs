using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pl_Covid_19_Anmeldung.datahandling.exceptions;

namespace Pl_Covid_19_Anmeldung.datahandling.entities
{
    public abstract class Entity
    {
        /// <summary>
        /// Gets all entry's from a class. As they can't change at runtime, they should be stored statically. But as they are for every class that inherits different, they can best be grabbed like this so load-time can be reduced.
        /// Use them like this: 1. Create a static Dictionary<string,FieldInfo> for your class and
        /// directly assign them to <see cref="getEntrys"/> with Yourclass
        /// </summary>
        protected abstract Dictionary<string, FieldInfo> Entrys();

        /// <summary>
        /// Tries to load the values of this object (that have been marked properly) from the given json supplier.
        /// </summary>
        /// <param name="supplier">The json-object that holds the values</param>
        /// <param name="required">All values that are required to load from the object</param>
        /// <param name="optional">All values that are optionally loaded from the object</param>
        /// <exception cref="RequiredEntitySerializeException">If any required field is not present</exception>
        /// <exception cref="EntitySerializeException">If anything went wrong while loading</exception>
        public void Load(JObject supplier, string[] required, params string[] optional) =>
        this.ExecuteActionForRequiredAndOptional((field, name, isRequired) =>
        {
            try
            {
                // Loads the object
                JToken obj = supplier[name];

                // Checks if the value is given
                if (obj == null)
                    // Returns null if the field is not required; otherwise an exception
                    return isRequired ? new RequiredEntitySerializeException(name) : null;

                // Tries to load the value for the field and set it
                field.SetValue(this, obj.ToObject(field.FieldType));
            }
            catch
            {
                // Failed to load some field
                return new EntitySerializeException(name);
            }
            return null;
        }, required, optional);

        /// <summary>
        /// Tries to save all entrys from this object (That have been marked properly) to the given supplier.
        /// </summary>
        /// <param name="supplier">The object that will hold all those objects</param>
        /// <param name="required">All values that must be saved from the entity</param>
        /// <param name="optional">All values that are optional and can be left out while saving</param>
        /// <exception cref="RequiredEntitySerializeException">If any required field is not present</exception>
        /// <exception cref="EntitySerializeException">If anything went wrong while saving.</exception>
        public void Save(JObject supplier, string[] required, params string[] optional) =>
        this.ExecuteActionForRequiredAndOptional((field, name, isRequired) =>
        {
            try
            {
                // Gets the value
                object value = field.GetValue(this);

                // Checks if the value is not given
                if(value == null)
                    // Returns null if the field is not required; otherwise an exception
                    return isRequired ? new RequiredEntitySerializeException(name) : null;

                // Sets the value
                supplier[name] = JToken.FromObject(value);
            }
            catch
            {
                // Failes to load a field or save it for some reason
                return new EntitySerializeException(name);
            }
            return null;
        }, required, optional);

        /// <summary>
        /// Executes the given action for all required and optional values.
        /// If a required action failes, the returned exception will be thrown
        /// </summary>
        /// <param name="supplier">The action to execute with all elements</param>
        /// <param name="required">All elements that are required to succed</param>
        /// <param name="optional">All elements that are optional to succed</param>
        /// <exception cref="Exception">Can throw any exception that will be returned by the supplier. Throws if any required action failes</exception>
        private void ExecuteActionForRequiredAndOptional(Func<FieldInfo, string, bool, Exception> supplier, string[] required, params string[] optional)
        {
            // Gets the entrys
            var entrys = this.Entrys();

            // Iterates over all values
            for(int i=0;i<required.Length + optional.Length; i++)
            {
                // If the field is required
                bool isRequired = i < required.Length;

                // Gets the name
                string name = isRequired ? required[i] : optional[i - required.Length];

                // Gets the entry
                FieldInfo field = entrys[name];

                // Executes the callback to execute some action
                var exc = supplier(field, name, isRequired);

                // Checks if the action failed
                if (exc != null)
                    // Throws the exception
                    throw exc;
            }
        }

        /// <summary>
        /// Searches all entrys (Fields that are convertable into json) and returns a dictionary with their names (to parse from and to json)
        /// </summary>
        /// <param name="cls">The base class to parse from</param>
        /// <returns>A dictionary with all entrys</returns>
        protected static Dictionary<string, FieldInfo> GetEntrys(Type cls)
        {
            // 1. Selects all fields from the given class with their first EntityInfo attribute
            // 2. Removes all invalid fields (Multiple or zero EntityInfo-attributes)
            // 3. Sort them
            // 4. Convert them to a dictonary
            var entrys = cls.GetFields().Select(field =>
                {
                    // Gets all attributes
                    var attris = field.GetCustomAttributes(typeof(EntityInfoAttribute), false);

                    // Returns if found the first attribute; otherwise null
                    return new KeyValuePair<string, FieldInfo>(
                        attris.Length == 1 ? ((EntityInfoAttribute)attris[0]).JsonName : null,
                        field
                    );
                })
                .Where(i => i.Key != null)
                .OrderBy(i => i.Key)
                .ToDictionary(x => x.Key, x => x.Value);

            // Checks if there are inheritable entrys that must be searched for
            if (!cls.BaseType.Equals(typeof(Entity)))
                // Appends the other entrys from the base class
                entrys.Concat(GetEntrys(cls.BaseType));

            return entrys;
        }

    }
}

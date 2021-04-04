using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pl_Covid_19_Anmeldung.utils
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    class EnumProperty : Attribute
    {

        // The key
        public string Key { get; private set; }
        // The value of the property
        public object Value { get; private set; }

        public EnumProperty(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <returns>The value of the property</returns>
        /// <exception cref="InvalidCastException">If the value of this property is not the same as the requested type</exception>
        public T GetValue<T>()
        {
            return (T)this.Value;
        }
    }
}

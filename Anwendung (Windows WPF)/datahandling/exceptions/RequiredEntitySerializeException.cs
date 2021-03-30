using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pl_Covid_19_Anmeldung.datahandling.exceptions
{
    class RequiredEntitySerializeException : EntitySerializeException
    {
        public RequiredEntitySerializeException(string name) : base(name) {}
    }
}

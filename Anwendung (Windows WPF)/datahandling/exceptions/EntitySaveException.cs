﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pl_Covid_19_Anmeldung.datahandling.exceptions
{
    class EntitySaveException : Exception
    {
        // The name of the entry that failed to save
        public string KeyName { get; private set; }

        public EntitySaveException(string keyName)
        {
            this.KeyName = keyName;
        }

    }
}

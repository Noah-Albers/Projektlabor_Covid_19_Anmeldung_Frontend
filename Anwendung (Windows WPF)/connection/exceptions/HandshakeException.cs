using System;

namespace Pl_Covid_19_Anmeldung.connection.exceptions
{
    class HandshakeException : Exception
    {
        public HandshakeExceptionType Type { get; private set; }

        public HandshakeException(HandshakeExceptionType type)
        {
            this.Type = type;
        }
    }

    enum HandshakeExceptionType
    {
        IO,
        RSA_DECRYPT
    }
}

using System;

namespace Socks.Exceptions
{
    class SocksRequestException : SocksException
    {
        public SocksRequestException(string message)
            : base(message) { }

        public SocksRequestException(string format, params object[] args)
            : base(format, args) { }

        public SocksRequestException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

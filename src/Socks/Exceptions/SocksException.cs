using System;

namespace Socks.Exceptions
{
    public class SocksException : Exception
    {
        public SocksException(string message)
            : base(message) { }

        public SocksException(string message, params object[] format)
            : base(string.Format(message, format)) { }

        public SocksException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
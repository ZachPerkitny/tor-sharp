/*
    A TCP Server supporting SOCKS4, SOCKS4a and SOCKS5 Protocols
    Copyright (C) 2018 Zach Perkitny

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
/*
    Provides processing for SOCKS4, SOCKS4a and SOCKS5 Requests
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
using System.Collections.Generic;
using System.Net.Sockets;
using Socks.Enum;
using Socks.Exceptions;

namespace Socks
{
    /*
     * SOCKS Connection Requests
     * --------------------------
     * 
     * SOCKS 4 (SOCKS Client -> SOCKS Server)
     *              +----+----+----+----+----+----+----+----+----+----+....+----+
     *              | VN | CD | DSTPORT |      DSTIP        | USERID       |NULL|
     *              +----+----+----+----+----+----+----+----+----+----+....+----+
     * # of bytes:    1    1
     * 
     * SOCKS 4 (SOCKS Server -> SOCKS Client)
     * 		        +----+----+----+----+----+----+----+----+
     * 		        | VN | CD | DSTPORT |      DSTIP        |
     * 		        +----+----+----+----+----+----+----+----+
     *  # of bytes:	   1    1      2              4
     */

    internal class SocksConnectionRequest
    {
        private readonly Socket _socket;

        public SocksConnectionRequest(Socket socket)
        {
            _socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        public CommandCode CommandCode { get; private set; }

        public ushort Port { get; private set; }

        public byte[] DestinationAddress { get; private set; }

        public bool DomainSpecified { get; private set; } = false;

        public byte[] UserID { get; private set; }

        public SocksVersion SocksVersion { get; private set; }

        public void Process()
        {
            SocksVersion = (SocksVersion)Recieve(1)[0];
            switch (SocksVersion)
            {
                case SocksVersion.Socks4:
                {
                    CommandCode = (CommandCode)Recieve(1)[0];
                    Port = ReadPort();
                    byte[] ipv4Address = Recieve(4);
                    UserID = ReadNullTerminated();

                    // SOCKS4a allows for a domain name
                    // to be included
                    if (IsSocks4aAddress(ipv4Address))
                    {
                        DestinationAddress = ReadNullTerminated();
                        DomainSpecified = true;
                    }
                    else
                    {
                        DestinationAddress = ipv4Address;
                    }
                    break;
                }
                case SocksVersion.Socks5:
                default:
                    break;
            }
        }

        public void SendSuccessResponse()
        {
            SendResponse(SocksStatus.RequestGranted);
        }

        public void SendErrorResponse()
        {
            SendResponse(SocksStatus.RequestRejectedOrFailed);
        }

        public bool IsConnectRequest()
        {
            return CommandCode == CommandCode.Stream;
        }

        public bool IsBindRequest()
        {
            return CommandCode == CommandCode.Binding;
        }

        private ushort ReadPort()
        {
            byte[] port = Recieve(2);
            // in network byte order
            return (ushort)(((port[0] << 8) | port[1]) + (((port[0] & 0x80) == 1) ? ushort.MinValue : 0));
        }

        private bool IsSocks4aAddress(byte[] ipv4Address)
        {
            for (int i = 0; i < 3; i++)
            {
                if (ipv4Address[i] != 0x0)
                {
                    return false;
                }
            }

            return ipv4Address[3] != 0x0;
        }

        private byte[] ReadNullTerminated()
        {
            List<byte> buffer = new List<byte>();
            while (true)
            {
                byte b = Recieve(1)[0];

                if (b == 0)
                {
                    break;
                }

                buffer.Add(b);
            }

            return buffer.ToArray();
        }

        private byte[] Receive(int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (offset < count)
            {
                int received = _socket.Receive(buffer, offset, buffer.Length - offset, 0);

                if (received == 0)
                {
                    throw new SocksRequestException("No Bytes Received, Expected {0}, Got {1}", count, offset);
                }

                offset += received;
            }

            return buffer;
        }

        private void SendResponse(SocksStatus status)
        {
            try
            {
                byte[] response;
                switch (SocksVersion)
                {
                    case SocksVersion.Socks4:
                        response = CreateSocks4Response(status);
                        break;
                    case SocksVersion.Socks5:
                    default:
                        response = null;
                        break;
                }

                _socket.Send(response);
            }
            catch (Exception)
            {
                // todo(zvp): Log it
            }
        }

        private byte[] CreateSocks4Response(SocksStatus status)
        {
            return new byte[] { 0x00, (byte)status, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        }
    }
}

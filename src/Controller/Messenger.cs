/*
    TorController - Uses the TOR Control Protocol to communicate with the TOR Process
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
using System.Net.Sockets;
using System.Text;
using TorController.Enum;
using TorController.Pocos;

namespace TorController
{
    /*
     * TC: A Tor control protocol
     * Tor Response Grammar (2)
     * 
     * Commands from controller to Tor (2.2)
     * -------------------------------------
     * Command = Keyword Arguments CRLF / "+" Keyword Arguments CRLF Data
     * Keyword = 1*ALPHA
     * Arguments = *(SP / VCHAR)
     * 
     * Replies from Tor to the controller (2.3)
     * ----------------------------------------
     * Reply = *(MidReplyLine / DataReplyLine) EndReplyLine
     * 
     * DataReplyLine = "+" ReplyLine Data
     * EndReplyLine = SP ReplyLine
     * MidReplyLine = "-" ReplyLine
     * ReplyLine = StatusCode [ SP ReplyText ]  CRLF
     * ReplyText = XXXX
     * StatusCode = XXXX
     * 
     * General-use tokens (2.4)
     * ------------------------
     * Data = *DataLine "." CRLF
     * DataLine = CRLF / "." 1*LineItem CRLF / NonDotItem *LineItem CRLF
     * LineItem = NonCR / 1*CR NonCRLF
     * NonDotItem = NonDotCR / 1*CR NonCRLF
     */

    internal class Messenger : IDisposable
    {
        private readonly ushort _port;
        private readonly Socket _socket;

        private bool _disposed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        public Messenger(ushort port)
        {
            _port = port;
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                _socket.Connect("localhost", _port);

                return true;
            }
            catch (Exception)
            {
                // log it, idk  
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public void Send(Command command)
        {
            try
            {
                byte[] bCommand = BuildCommand(command);
                _socket.Send(bCommand);

                ReadReply();
            }
            catch (Exception)
            {
                // log it, idk
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            _socket.Disconnect(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _socket.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private byte[] BuildCommand(Command command)
        {
            // Command = Keyword Arguments CRLF / "+" Keyword Arguments CRLF Data
            // Keyword = 1 * ALPHA
            // Arguments = *(SP / VCHAR)
            return Encoding.ASCII.GetBytes(
                $"{command.Keyword} {string.Join(" ", command.Arguments)}\r\n");
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadReply()
        {
            while (true)
            {
                Status status = ReadStatus();
                switch ((char)Receive(1)[0])
                {
                    case '-':
                        ReadReplyText();
                        break;
                    case '+':
                        ReadReplyText();
                        break;
                    default: // end reply line, can return here
                        ReadReplyText();
                        return;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string ReadReplyText()
        {
            StringBuilder replyTextBuilder = new StringBuilder();
            char c;
            while (true)
            {
                c = (char)Receive(1)[0];
                if (c == '\r' && (char)Receive(1)[0] == '\n')
                {
                    return replyTextBuilder.ToString();
                }

                replyTextBuilder.Append(c);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Status ReadStatus()
        {
            string status = Encoding.ASCII.GetString(Receive(3));
            return (Status)System.Enum.Parse(typeof(Status), status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] Receive(int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (offset < count)
            {
                int received = _socket.Receive(buffer, offset, buffer.Length - offset, 0);

                offset += received;

                if (received == 0)
                {
                    throw new Exception();
                }
            }

            return buffer;
        }
    }
}

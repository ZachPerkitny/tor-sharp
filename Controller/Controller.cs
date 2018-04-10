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
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TorController.Enum;

namespace TorController
{
    public class Controller
    {
        private const int BUFFER_SIZE = 1024;

        private readonly ushort _controlPort;
        private readonly Socket _controlSocket;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlPort"></param>
        public Controller(ushort controlPort = 9051)
        {
            _controlPort = controlPort;
            _controlSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                _controlSocket.Connect(
                    "localhost",
                    _controlPort);

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
        /// <param name="keyword"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetConfiguration(string keyword, object value)
        {
            Response response = SendCommand("SETCONF {0}={1}", keyword, value);
            return response != null && response.IsOk();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public Tuple<string, string> GetConfiguration(string keyword)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string>> GetConfiguration(IEnumerable<string> keywords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Authenticate(string password = "")
        {
            Response response = SendCommand("AUTHENTICATE \"{0}\"", password);
            if (response != null && response.IsOk())
            {
                IsAuthenticated = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public bool Signal(Signal signal)
        {
            Response response = SendCommand("SIGNAL {0}", signal);
            return response != null && response.IsOk();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Response SendCommand(string format, params object[] args)
        {
            try
            {
                byte[] command = Encoding.ASCII.GetBytes(
                    string.Format($"{format}\r\n", args));
                _controlSocket.Send(command);

                byte[] response = RecieveUntilCRLF();
                return Response.Parse(Encoding.ASCII.GetString(response));
            }
            catch (Exception)
            {
                // log it
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private byte[] Recieve(int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (offset < count)
            {
                int received = _controlSocket.Receive(buffer, offset, buffer.Length - offset, 0);

                offset += received;

                if (received == 0)
                {
                    throw new Exception();
                }
            }

            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private byte[] RecieveUntilCRLF()
        {
            List<byte> response = new List<byte>();
            bool lastCR = false;

            while (true)
            {
                byte b = Recieve(1)[0];
                response.Add(b);
            }

            return response.ToArray();
        }
    }
}
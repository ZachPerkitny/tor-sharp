/*
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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Socks.Exceptions;

namespace Socks
{
    internal class SocksClient
    {
        private const int BUFFER_SIZE = 4096;

        private readonly Socket _clientSocket;
        private Socket _remoteSocket;

        private readonly byte[] _clientBuffer;
        private readonly byte[] _remoteBuffer;

        private bool _closed = false;
        private bool _processed = false;

        private readonly object _locker = new object();

        public SocksClient(Socket socket)
        {
            _clientSocket = socket ?? throw new ArgumentNullException(nameof(socket));
            _clientBuffer = new byte[BUFFER_SIZE];
            _remoteBuffer = new byte[BUFFER_SIZE];
        }

        public async Task<bool> Connect()
        {
            if (_processed)
            {
                return true;
            }

            if (_closed)
            {
                return false;
            }

            SocksConnectionRequest request = new SocksConnectionRequest(_clientSocket);
            try
            {
                request.Process();

                if (!request.IsConnectRequest())
                {
                    request.SendErrorResponse();
                    Close();
                    return false;
                }

                _remoteSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);
                
                await _remoteSocket.ConnectAsync(
                    new IPAddress(request.DestinationAddress),
                    request.Port);

                _clientSocket.BeginReceive(_clientBuffer, 0, _clientBuffer.Length, 0, OnReceivedDataFromClient, _clientSocket);
                _remoteSocket.BeginReceive(_remoteBuffer, 0, _remoteBuffer.Length, 0, OnReceivedDataFromRemote, _remoteSocket);

                request.SendSuccessResponse();
                _processed = true;
                return true;
            }
            catch (SocksRequestException)
            {
                // log it
            }

            return false;
        }

        public void Close()
        {
            lock (_locker)
            {
                if (_closed)
                {
                    return;
                }

                _clientSocket.Close();
                if (_remoteSocket != null)
                {
                    _remoteSocket.Close();
                }

                _closed = true;
            } 
        }

        private void OnReceivedDataFromClient(IAsyncResult asyncResult)
        {
            lock (_locker)
            {
                if (_closed)
                {
                    return;
                }

                int received = ((Socket)asyncResult.AsyncState).EndReceive(asyncResult, out SocketError socketError);
                if (received == 0 || socketError != SocketError.Success)
                {
                    Close();
                    return;
                }

                _remoteSocket.Send(_clientBuffer);
                _clientSocket.BeginReceive(_clientBuffer, 0, _clientBuffer.Length, 0, OnReceivedDataFromClient, _clientSocket);
            }
        }

        private void OnReceivedDataFromRemote(IAsyncResult asyncResult)
        {
            lock (_locker)
            {
                if (_closed)
                {
                    return;
                }

                int received = ((Socket)asyncResult.AsyncState).EndReceive(asyncResult, out SocketError socketError);
                if (received == 0 || socketError != SocketError.Success)
                {
                    Close();
                    return;
                }

                _clientSocket.Send(_remoteBuffer);
                _remoteSocket.BeginReceive(_remoteBuffer, 0, _remoteBuffer.Length, 0, OnReceivedDataFromRemote, _remoteSocket);
            }
        }
    }
}

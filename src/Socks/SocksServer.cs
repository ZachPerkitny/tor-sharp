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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Socks.Exceptions;

namespace Socks
{
    public class SocksServer
    {
        private const int BACKLOG = 1000;
        private const int BUFFER_SIZE = 256;

        private readonly TcpListener _listener;
        private Task _acceptTask;

        private readonly List<SocksClient> _clients = new List<SocksClient>();

        private bool _running = false;

        public SocksServer(IPAddress ipAddress, int port)
        {
            _listener = new TcpListener(ipAddress, port);
        }

        public void Start()
        {
            if (!_running)
            {
                _listener.Start(BACKLOG);
                _acceptTask = Task.Run(Accept);
                _running = true;
            }
        }

        public void Stop()
        {
            if (_running)
            {
                _listener.Stop();
                _running = false;
            }
        }

        private async Task Accept()
        {
            while (_running)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    SocksClient socksClient = new SocksClient(client.Client);
                    if (await socksClient.Connect())
                    {
                        _clients.Add(socksClient);
                    }
                }
                catch (SocksRequestException ex)
                {
                    Console.WriteLine("SocksRequestException: {0}", ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex.Message);
                }
            }
        }
    }
}

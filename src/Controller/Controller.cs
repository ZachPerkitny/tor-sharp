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
using TorController.Enum;
using TorController.Pocos;

namespace TorController
{
    public class Controller : IDisposable
    {
        private readonly Messenger _messenger;

        private bool _disposed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlPort"></param>
        public Controller(ushort controlPort = 9051)
        {
            _messenger = new Messenger(controlPort);
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
            return _messenger.Connect();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetConfiguration(string keyword, object value)
        {
            //Response response = SendCommand("SETCONF {0}={1}", keyword, value);
            //return response != null && response.IsOk();
            throw new NotImplementedException();
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
            _messenger.Send(new Command
            {
                Keyword = "AUTHENTICATE",
                Arguments = new object[] { $"\"{password}\"" }
            });

            //Response response = SendCommand("AUTHENTICATE \"{0}\"", password);
            //if (response != null && response.IsOk())
            //{
            //    IsAuthenticated = true;
            //    return true;
            //}

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public bool Signal(Signal signal)
        {
            _messenger.Send(new Command
            {
                Keyword = "SIGNAL",
                Arguments = new object[] { signal }
            });

            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _messenger.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
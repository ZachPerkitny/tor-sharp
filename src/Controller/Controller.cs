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
using System.Linq;
using TorController.Enum;
using TorController.Exceptions;
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
        public void SetConfiguration(string keyword, object value)
        {
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "SETCONF",
                Arguments = new object[] { $"{keyword}={value}" }
            });

            ThrowIfNotOk(reply);
        }

        public void SetConfiguration(IEnumerable<Tuple<string, object>> keyValues)
        {
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "SETCONF",
                Arguments = keyValues.Select(kv => $"{kv.Item1}={kv.Item2}")
            });

            ThrowIfNotOk(reply);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public Tuple<string, string> GetConfiguration(string keyword)
        {
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "GETCONF",
                Arguments = new object[] { keyword }
            });

            ThrowIfNotOk(reply);

            ReplyLine replyLine = reply.ReplyLines.First();
            return GetKeyValueFromReplyLine(replyLine);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string>> GetConfiguration(IEnumerable<string> keywords)
        {
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "GETCONF",
                Arguments = keywords
            });

            ThrowIfNotOk(reply);

            List<Tuple<string, string>> keyValues = new List<Tuple<string, string>>();
            foreach (ReplyLine replyLine in reply.ReplyLines)
            {
                Tuple<string, string> keyValue = GetKeyValueFromReplyLine(replyLine);
                keyValues.Add(keyValue);
            }

            return keyValues;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        public void Authenticate(string password = "")
        {
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "AUTHENTICATE",
                Arguments = new object[] { $"\"{password}\"" }
            });

            ThrowIfNotOk(reply);

            IsAuthenticated = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        public void Signal(Signal signal)
        {
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "SIGNAL",
                Arguments = new object[] { signal }
            });

            ThrowIfNotOk(reply);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="replyLine"></param>
        /// <returns></returns>
        private Tuple<string, string> GetKeyValueFromReplyLine(ReplyLine replyLine)
        {
            if (replyLine.ReplyText.Contains("="))
            {
                string[] keyValue = replyLine.ReplyText.Split(new char[] { '=' });
                return Tuple.Create(keyValue[0], keyValue[1]);
            }
            else
            {
                return Tuple.Create(replyLine.ReplyText, string.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reply"></param>
        private void ThrowIfNotOk(Reply reply)
        {
            if (!reply.IsOk)
            {
                ReplyLine replyLine = reply.ReplyLines.First();
                throw new ControllerException(replyLine.Status, replyLine.ReplyText);
            }
        }
    }
}
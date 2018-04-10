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
    /*
     * TC: A Tor control protocol
     * 
     * General Purpose (Augmented Backus Naus Form) (2.1)
     * Nonterminals atom, qcontent from RFC2822
     * String = DQUOTE *qcontent DQUOTE
     * 
     * Tor Command Grammar (Augmented Backus Naus Form) (3)
     * 
     * SETCONF 3.1
     * -----------
     * "SETCONF" 1*(SP keyword ["=" String]) CRLF
     * 
     * RESETCONF 3.2
     * -------------
     * "RESETCONF" 1*(SP keyword ["=" String]) CRLF
     * 
     * GETCONF 3.3
     * -----------
     * "GETCONF" 1*(SP keyword) CRLF
     * 
     * SETEVENTS 3.4
     * --------------
     * "SETEVENTS" [SP "EXTENDED"] *(SP EventCode) CRLF
     * 
     * EventCode = "CIRC" / "STREAM" / "ORCONN" / "BW" / "DEBUG" /
     *      "INFO" / "NOTICE" / "WARN" / "ERR" / "NEWDESC" / "ADDRMAP" /
     *      "AUTHDIR_NEWDESCS"
     * 
     * AUTHENTICATE 3.5
     * ----------------
     * "AUTHENTICATE" [ SP 1*HEXDIG / QuotedString ] CRLF
     * 
     * SAVECONF 3.6
     * ------------
     * "SAVECONF" CRLF
     * 
     * SIGNAL 3.7
     * ----------
     * "SIGNAL" SP Signal CRLF
     * 
     * Signal = "RELOAD" / "SHUTDOWN" / "DUMP" / "DEBUG" / "HALT" /
     *      "HUP" / "INT" / "USR1" / "USR2" / "TERM" / "NEWNYM"
     */

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
            // "SETCONF" 1*(SP keyword ["=" String]) CRLF
            SendKeyValue("SETCONF", keyword, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        public void SetConfiguration(IEnumerable<Tuple<string, object>> keyValues)
        {
            // "SETCONF" 1*(SP keyword ["=" String]) CRLF
            SendKeyValues("SETCONF", keyValues);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="value"></param>
        public void ResetConfiguration(string keyword, object value)
        {
            // "RESETCONF" 1*(SP keyword ["=" String]) CRLF
            SendKeyValue("RESETCONF", keyword, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        public void ResetConfiguration(IEnumerable<Tuple<string, object>> keyValues)
        {
            // "RESETCONF" 1*(SP keyword ["=" String]) CRLF
            SendKeyValues("RESETCONF", keyValues);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public Tuple<string, string> GetConfiguration(string keyword)
        {
            // "GETCONF" 1*(SP keyword) CRLF
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
            // "GETCONF" 1*(SP keyword) CRLF
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
        /// <param name="extended"></param>
        /// <param name="eventCodes"></param>
        public void SetEvents(bool extended, IEnumerable<EventCode> eventCodes)
        {
            // "SETEVENTS" [SP "EXTENDED"] *(SP EventCode) CRLF
            // See 3.4 for EventCode Production rule
            List<object> arguments = new List<object>();
            if (extended)
            {
                arguments.Add("EXTENDED");
            }

            if (eventCodes != null)
            {
                arguments.Concat((IEnumerable<object>)eventCodes);
            }

            Reply reply = _messenger.Send(new Command
            {
                Keyword = "SETEVENTS",
                Arguments = arguments
            });

            ThrowIfNotOk(reply);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        public void Authenticate(string password = "")
        {
            // "AUTHENTICATE" [ SP 1*HEXDIG / QuotedString ] CRLF
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
        private void SaveConfiguration()
        {
            // "SAVECONF" CRLF
            Reply reply = _messenger.Send(new Command
            {
                Keyword = "SAVECONF"
            });

            ThrowIfNotOk(reply);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        public void Signal(Signal signal)
        {
            // "SIGNAL" SP Signal CRLF
            // See 3.7 for Signal Production rule
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
        /// <param name="command"></param>
        /// <param name="keyword"></param>
        /// <param name="value"></param>
        private void SendKeyValue(string command, string keyword, object value)
        {
            // See SETCONF (3.1), RESETCONF (3.2) AND MAPADDRESS (3.8)
            if (string.IsNullOrEmpty(keyword))
            {
                throw new ControllerException("Invalid Keyword, Expected atleast one character.");
            }

            Reply reply = _messenger.Send(new Command
            {
                Keyword = command,
                Arguments = new object[] { $"{keyword}=\"{value}\"" }
            });

            ThrowIfNotOk(reply);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="keyValues"></param>
        private void SendKeyValues(string command, IEnumerable<Tuple<string, object>> keyValues)
        {
            // See SETCONF (3.1), RESETCONF (3.2) AND MAPADDRESS (3.8)
            if (keyValues == null || keyValues.Count() < 1)
            {
                throw new ControllerException("Expected at least one Key-Value Pair");
            }

            Reply reply = _messenger.Send(new Command
            {
                Keyword = command,
                Arguments = keyValues.Select(kv =>
                {
                    if (string.IsNullOrEmpty(kv.Item1))
                    {
                        throw new ControllerException("Invalid Keyword, Expected atleast one character.");
                    }

                    return $"{kv.Item1}=\"{kv.Item2}\"";
                })
            });

            ThrowIfNotOk(reply);
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
                throw new CommandException(replyLine.Status, replyLine.ReplyText);
            }
        }
    }
}
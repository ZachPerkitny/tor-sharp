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
using System.Text;
using TorController.Enum;

namespace TorController
{
    /*
     * TC: A Tor control protocol
     * Tor Response Grammar (2)
     * 
     * Replies from Tor to the controller (2.3)
     * ----------------------------------f
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
     * ------------------
     * Data = *DataLine "." CRLF
     * DataLine = CRLF / "." 1*LineItem CRLF / NonDotItem *LineItem CRLF
     * LineItem = NonCR / 1*CR NonCRLF
     * NonDotItem = NonDotCR / 1*CR NonCRLF
     */

    public class Response
    {
        private readonly string _response;
        private int _pos;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static Response Parse(string res)
        {
            Response response = new Response(res);
            response.ParseReply();

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Tuple<Status, string>> Reply { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsOk()
        {
            return Reply.Any(reply => reply.Item1 == Status.OK);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        private Response(string response)
        {
            Reply = new List<Tuple<Status, string>>();
            _response = response ?? throw new ArgumentNullException(nameof(response));
            _pos = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ParseReply()
        {
            while (_pos < _response.Length)
            {
                switch (_response[_pos])
                {
                    case '+':
                        ParseReplyLine();
                        break;
                    case '-':
                    case ' ':
                        ParseReplyLine();
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ParseReplyLine()
        {
            // ReplyLine = StatusCode [ SP ReplyText ]  CRLF

            _pos += 1; // consume character

            StringBuilder statusCodeBuilder = new StringBuilder();
            while (_response[_pos] != ' ')
            {
                if (_pos >= _response.Length)
                {
                    throw new Exception();
                }

                if (!Char.IsDigit(_response[_pos]))
                {
                    throw new Exception();
                }

                statusCodeBuilder.Append(_response[_pos]);
                _pos += 1;
            }

            _pos += 1; // consume space

            StringBuilder replyTextBuilder = new StringBuilder();
            while (true)
            {
                if (_pos >= _response.Length - 1)
                {
                    throw new Exception();
                }

                if (_response[_pos] == '\r' && _response[_pos + 1] == '\n')
                {
                    _pos += 2;
                    break;
                }

                _pos += 1;
            }

            Reply.Add(Tuple.Create(
                (Status)int.Parse(statusCodeBuilder.ToString()), replyTextBuilder.ToString()));
        }
    }
}

/*
    TorControl - Uses the TOR Control Protocol to communicate with the TOR Process
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

using System.Collections.Generic;
using System.Linq;
using TorControl.Enum;

namespace TorControl.Pocos
{
    internal class Reply
    {
        public IEnumerable<ReplyLine> ReplyLines { get; set; }

        public bool IsOk
        {
            get
            {
                if (ReplyLines == null)
                {
                    return false;
                }

                return ReplyLines.Any(r => r.Status == Status.OK);
            }
        }

        public bool IsAsync
        {
            get
            {
                if (ReplyLines == null)
                {
                    return false;
                }

                return ReplyLines.Any(r => r.Status == Status.AsynchronousEventNotification);
            }
        }
    }
}

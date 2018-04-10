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

namespace TorController.Enum
{
    /* TC: A Tor control protocol
     * Tor Replies (4)
     * ---------------
     * 2yz Positive Completion Reply
     * 4yz Temporary Negative Completion Reply
     * 5yz Permanent Negative Completion Reply
     * 6yz Asynchronous Reply
     * 
     * Non Standard, Used Internally by Event Loop
     * 9yz TorController Error
     */
    
    public enum Status
    {
        OK = 250,
        Unnecessary = 251,
        ResourceExhausted = 451,
        SyntaxError = 500,
        UnrecognizedCommand = 510,
        UnimplementedCommand = 511,
        SyntaxErrorInCommandArg = 512,
        UnrecognizedCommandArg = 513,
        AuthRequired = 514,
        BadAuthentication = 515,
        UnspecifiedTorError = 550,
        // Something went wrong inside Tor, so that the client's
        // request couldn't be fulfilled.
        InternalError = 551,
        // Configuration key, stream id, circuit id,
        // event mentioned in the command did not
        // actually exist
        UnrecognizedEntity = 552,
        // The client tried to set a configuration option to an
        // incorrect, ill-formed, or impossible value.
        InvalidConfigurationValue = 553,
        InvalidDescriptor = 554,
        UnmanagedEntity = 555,
        AsynchronousEventNotification = 650,
        // ------------------
        // NON STANDARD START
        // ------------------
        // Start Internal Tor Controller Statuses
        MessengerError = 999
    }
}

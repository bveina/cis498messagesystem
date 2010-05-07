using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

/************************************************************************
 * Portions of this code are adapted from the following document:       *
 * Title: "A Chat Application using Asynchronous TCP Sockets"           *
 * Author: Hitesh Sharma                                                *
 * URL: http://www.codeproject.com/KB/IP/ChatAsynchTCPSockets.aspx      *
 ************************************************************************/

namespace TFMS_Space
{
    /// <summary>
    /// Contains all information about a person connected to the server
    /// </summary>
    public class TFMS_ClientInfo
    {
        /// <summary>
        /// Socket used to comunicate between this client and the server
        /// </summary>
        public Socket socket;

        /// <summary>
        /// Holds this client's username
        /// </summary>
        public string strName;

        /// <summary>
        /// Dedicated buffer for the recipt of TCP messages
        /// </summary>
        public byte[] buffer;

        /// <summary>
        /// TFMS_ClientInfo constructor:
        /// - associates a client with a Socket and a name
        /// </summary>
        /// <param name="inSocket">the socket to associate with this client</param>
        /// <param name="inName">the name given by the client</param>
        public TFMS_ClientInfo(Socket inSocket, string inName)
        {
            socket = inSocket;
            strName = inName;
            buffer = new byte[TFMS_Constants.BUFFER_SIZE];
        }
    }
}

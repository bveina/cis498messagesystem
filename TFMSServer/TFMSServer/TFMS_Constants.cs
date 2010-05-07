using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/************************************************************************
 * Portions of this code are adapted from the following document:       *
 * Title: "A Chat Application using Asynchronous TCP Sockets"           *
 * Author: Hitesh Sharma                                                *
 * URL: http://www.codeproject.com/KB/IP/ChatAsynchTCPSockets.aspx      *
 ************************************************************************/

namespace TFMS_Space
{
    /// <summary>
    /// Holds list of constants used in both the client and server classes
    /// </summary>
    public class TFMS_Constants
    {
        /// <summary>
        /// Defines the default port number to connect from
        /// </summary>
        public const int PORT_NUM = 4242;

        /// <summary>
        /// Defines the default value for thread delay
        /// </summary>
        public const int DELAY_TIME = 100;

        /// <summary>
        /// Defines the default segment size (in bytes)
        /// </summary>
        public const int BUFFER_SIZE = 1024;
    }
}

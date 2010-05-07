using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Drawing;
using System.Threading;


/************************************************************************
 * Portions of this code are adapted from the following document:       *
 * Title: "A Chat Application using Asynchronous TCP Sockets"           *
 * Author: Hitesh Sharma                                                *
 * URL: http://www.codeproject.com/KB/IP/ChatAsynchTCPSockets.aspx      *
 ************************************************************************/

namespace TFMS_Space
{
    /// <summary>
    /// Defines the delegate that handles different types of messages
    /// </summary>
    /// <param name="data">data from the received message</param>
    public delegate void TFMS_MessageRecieved(TFMS_Data data);

    /// <summary>
    /// The commands for interaction between the server and the client
    /// </summary>
    public enum TFMS_Command
    {
        Login,      // Log into the server
        Logout,     // Log out of the server
        List,       // Get a list of users logged in to the server
        Message,    // Send a text message to all clients currently logged in
        MsgLen,     // Send a long message to all clients currently logged in
        Null,       // default case
    }
}
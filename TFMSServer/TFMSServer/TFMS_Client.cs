using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
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
    /// The TFMS_Client class allows users to log in to a central server to distribute messages with other clients
    /// A TFMS_Client sends a message to the TFMS_Server, which in turn broadcasts the message to all clients currently logged in
    /// </summary>
    public class TFMS_Client
    {
        #region TFMS_Client class variables

        /// <summary>
        /// The local port to try to contact the server from
        /// </summary>
        public int serverPort;

        /// <summary>
        /// The socket for communicating with the server
        /// </summary>
        public Socket clientSocket;

        /// <summary>
        /// The username of this client
        /// </summary>
        public string strName;

        /// <summary>
        /// Data buffer for receiving data from the server
        /// </summary>
        public byte[] byteData;

        /// <summary>
        /// Delegates for handling logon, logoff, relay, and list requests
        /// </summary>
        public event TFMS_MessageRecieved loginReceived;
        public event TFMS_MessageRecieved logoffReceived;
        public event TFMS_MessageRecieved dataReceived;
        public event TFMS_MessageRecieved listReceived;
        public event TFMS_MessageRecieved disconnectDetected;

        #endregion

        #region TFMS_Client constructors

        /// <summary>
        /// Default constructor: creates a new TFMS_Client object with the default port number and a null user name
        /// </summary>
        public TFMS_Client()
            : this(TFMS_Constants.PORT_NUM, "") { }

        /// <summary>
        /// Create a new TFMS_Client object, given a specified port number and user name
        /// </summary>
        /// <param name="port">the port number to initiate contact with the server)</param>
        /// <param name="Name">the name you want to use to comunicate</param>
        public TFMS_Client(int port, string name)
        {
            // make sure the specifed port is within the legal range, otherwise set it to the default port number
            if (port < 1025 || port > 65535)
                serverPort = TFMS_Constants.PORT_NUM;
            else
                serverPort = port;

            clientSocket = null;
            strName = name;
            byteData = new byte[TFMS_Constants.BUFFER_SIZE];
        }

        #endregion

        #region TFMS_Client public methods

        /// <summary>
        /// Attempt to connect to the server at the specified IP address
        /// </summary>
        /// <param name="IP_address">the IP address of the target server ("a.b.c.d" format)</param>
        /// <returns>true if the connection was succesful, otherwise false</returns>
        public bool connect(string IP_address)
        {
            try
            {
                // create the socket
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // get the IP addresses, then connect them with the socket
                IPAddress serverip = IPAddress.Parse(IP_address);
                IPEndPoint ipEnd = new IPEndPoint(serverip, serverPort);
                clientSocket.Connect(ipEnd);

                // try to login after connecting to the server
                TFMS_Data msgToSend = new TFMS_Data(TFMS_Command.Login, null, strName);
                clientSocket.Send(msgToSend.ToByte());

                // setup buffer to receive data from the server
                byteData = new byte[TFMS_Constants.BUFFER_SIZE];

                // start listening to the data asynchronously
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);

                // the client successfully connected
                Console.WriteLine("{0} successfully connected to the server!", msgToSend.strName);
                return true;
            }
            catch (Exception)
            {
                // the client failed to connect
                Console.WriteLine("Client at '{0}' failed to connect to the server!", IP_address);
                return false;
            }
        }

        /// <summary>
        /// Attempt to disconnect from the server by sending the Logout command
        /// </summary>
        public void disconnect()
        {
            try
            {
                // send the Logout command to the server
                byte[] data = (new TFMS_Data(TFMS_Command.Logout, null, strName)).ToByte();
                clientSocket.Send(data);
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Request a list of TFMS_Client objects that are currently connected to the server
        /// </summary>
        public void getClientList()
        {
            // prepare the List command
            byte[] data = (new TFMS_Data(TFMS_Command.List, null, strName)).ToByte();
            byte[] getList = (new TFMS_Data(TFMS_Command.MsgLen, string.Format("{0}", data.Length), strName)).ToByte();

            // send the client list request to the socket
            clientSocket.Send(getList);
            Thread.Sleep(TFMS_Constants.DELAY_TIME);

            // begin asynchronous send to the server
            clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
        }

        /// <summary>
        /// Send a message to the server that needs to be broadcast to all other clients
        /// </summary>
        /// <param name="path">the path drawn on the client's DrawingBox that will be relayed to other clients</param>
        public void sendMessage(string path)
        {
            // prepare the data by converting the message informatino into a byte array
            byte[] data = (new TFMS_Data(TFMS_Command.Message, path, strName)).ToByte();
            TFMS_Data lenToSend = new TFMS_Data(TFMS_Command.MsgLen, string.Format("{0}", data.Length), strName);

            // send the message to the socket
            clientSocket.Send(lenToSend.ToByte());
            Thread.Sleep(TFMS_Constants.DELAY_TIME);

            // begin sending the message to the server
            clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
        }

        /// <summary>
        /// When the result is received, end the message send
        /// </summary>
        /// <param name="result">the result of the TFMS_Client attempting to send a message to the server</param>
        public void OnSend(IAsyncResult result)
        {
            try
            {
                clientSocket.EndSend(result);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("This object has been disposed");
            }
        }

        /// <summary>
        /// OnReceive will interpret messages received from the TFMS_Server object and handle the possible messages
        /// </summary>
        /// <param name="result">the result of the response from the server</param>
        public void OnReceive(IAsyncResult result)
        {
            // assume default buffer size
            int buffSize = TFMS_Constants.BUFFER_SIZE;

            try
            {
                int numBytesReceived = 0;
                TFMS_Data msgReceived;

                try
                {
                    // attempt to get the number of bytes received from the server
                    numBytesReceived = clientSocket.EndReceive(result);
                }
                catch (Exception)
                {
                    disconnectDetected(null);
                    return;
                }


                // using the received number of bytes, create a new TFMS_Data object to handle incoming data
                if (numBytesReceived == 0)
                {
                    msgReceived = new TFMS_Data(TFMS_Command.Null, "", "");
                }
                else
                {
                    byte[] temp = new byte[numBytesReceived];
                    Array.Copy(byteData, temp, numBytesReceived);
                    msgReceived = new TFMS_Data(temp);
                }

                #region Interpret incoming message command

                // handlccordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case TFMS_Command.Login:
                        loginReceived(msgReceived);
                        break;

                    case TFMS_Command.Logout:
                        logoffReceived(msgReceived);
                        break;

                    case TFMS_Command.Message:
                        dataReceived(msgReceived);
                        break;

                    case TFMS_Command.MsgLen:
                        // this is the only place that a long message has to do work in the client class
                        // everything else is passed on to the client
                        buffSize = int.Parse(msgReceived.strMessage);
                        break;

                    case TFMS_Command.List:
                        listReceived(msgReceived);
                        break;

                    case TFMS_Command.Null:
                        break;
                }

                #endregion

                byteData = new byte[buffSize];

                // wait for next message from server.
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);

            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("This object has been disposed");
            }
            catch (SocketException ex)
            {
                // if there is a Socket exception, clear out this client from the socket and disconnect
                Console.WriteLine(ex.Message);
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    this.clientSocket = null;
            }
        }

        #endregion
    }
}

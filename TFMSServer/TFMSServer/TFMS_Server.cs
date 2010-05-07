using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

/************************************************************************
 * Portions of this code are adapted from the following document:       *
 * Title: "A Chat Application using Asynchronous TCP Sockets"           *
 * Author: Hitesh Sharma                                                *
 * URL: http://www.codeproject.com/KB/IP/ChatAsynchTCPSockets.aspx      *
 ************************************************************************/

namespace TFMS_Space
{
    /// <summary>
    /// The TFMS_Server class is responsible for distributing messages among the clients
    /// A TFMS_Client sends a message to the TFMS_Server, which in turn broadcasts the message to all clients currently logged in
    /// </summary>
    public class TFMS_Server
    {
        #region TFMS_Server class variables

        /// <summary>
        /// Holds the list of clients that are currently connected to the server
        /// </summary>
        private List<TFMS_ClientInfo> clientList;

        /// <summary>
        /// A single Socket that listens for new clients
        /// NOTE: future iterations of TFMS might want to create a separate socket for EACH client, not one for ALL clients
        /// </summary>
        private Socket serverSocket;

        /// <summary>
        /// Buffer for receiving connect requests
        /// </summary>
        private byte[] byteData;

        /// <summary>
        /// Port that the server listens on for incoming connections
        /// - each individual client is assigned its own port numbers
        /// </summary>
        private int listenPort;

        /// <summary>
        /// Delegates for handling logon, logoff, relay, and list requests
        /// </summary>
        public TFMS_MessageRecieved logonRequested;
        public TFMS_MessageRecieved logoffRequested;
        public TFMS_MessageRecieved relayRequested;
        public TFMS_MessageRecieved listRequested;

        #endregion

        #region TFMS_Server constructors

        /// <summary>
        /// Create a new TFMS_Server object
        /// - if no parameter is specified, open a connection on the default port number
        /// </summary>
        public TFMS_Server()
            : this(TFMS_Constants.PORT_NUM) { }

        /// <summary>
        /// Create a new TFMS_Server object
        /// </summary>
        /// <param name="port">user-specified port to open a connection on</param>
        public TFMS_Server(int port)
        {
            listenPort = port;
            byteData = new byte[TFMS_Constants.BUFFER_SIZE];
            clientList = new List<TFMS_ClientInfo>();
        }
        #endregion

        #region TFMS_Server public methods

        /// <summary>
        /// Creates a socket and listens for client connections to that socket
        /// </summary>
        /// <returns>true if the server starts, false if there is an error</returns>
        public bool startServer()
        {
            try
            {
                // create the socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // get the current incoming IP address and assign it to the socket
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                serverSocket.Bind(ipEndPoint);

                // begin listening for connecting clients
                serverSocket.Listen(5);
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                // if the server is successfully started, print a confirmation and return true
                Console.WriteLine("Begin accepting client connections:");
                return true;
            }
            catch (Exception)
            {
                // if the server cannot be started, print an error and return false
                Console.WriteLine("Error starting server!");
                return false;
            }
        }

        /// <summary>
        /// Provide a method for outside entities to access the current client list
        /// </summary>
        /// <returns>the list of clients currently logged in to the server</returns>
        public List<TFMS_ClientInfo> getClientList()
        {
            return clientList;
        }
        /// <summary>
        /// Begin accepting data from a client when it tries to connect to the server
        /// </summary>
        /// <param name="result">the result of the incoming connection attempt</param>
        public void OnAccept(IAsyncResult result)
        {
            //get the socket the client and server are comunicating through
            Socket clientSocket = serverSocket.EndAccept(result);

            // start listening for other clients
            serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

            //setup the async receive (receieve is getting actual messages)
            Console.WriteLine("Client connected!");
            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
        }

        /// <summary>
        /// this hadles the actual message forwarding ect.
        /// you should never call this function directly
        /// </summary>
        /// <param name="result">this encapsulates the Socket of the client</param>
        public void OnReceive(IAsyncResult result)
        {
            // get the socket that received data
            Socket clientSocket = (Socket)result.AsyncState;

            // get the ClientInfo from the connected client
            TFMS_ClientInfo CI = null;
            if (findIndexFromClient(clientList, clientSocket) >= 0)
                CI = clientList[findIndexFromClient(clientList, clientSocket)];


            Console.WriteLine("\n****************************************");
            Console.WriteLine("Data Received from {0}", getNamefromSocket(clientSocket));


            // attempt to get the data from the client and convert it into TFMS_Data object
            TFMS_Data msgReceived = extractData(clientSocket, CI, result);

            TFMS_Data msgToSend = new TFMS_Data();
            msgToSend.cmdCommand = msgReceived.cmdCommand;
            msgToSend.strName = msgReceived.strName;


            #region Interpret incoming message command

            // set the message to send, client info buffer size, and client info based on the incoming command
            switch (msgReceived.cmdCommand)
            {
                case TFMS_Command.Login:    // new client logging in
                    #region Login command

                    Console.WriteLine("Received login from {0}", getNamefromSocket(clientSocket));

                    // tell the server a logon was requested
                    logonRequested(msgReceived);

                    // add the new client to the current list of clients
                    TFMS_ClientInfo clientInfo = new TFMS_ClientInfo(clientSocket, msgReceived.strName);
                    clientList.Add(clientInfo);

                    // pass the incoming client's name to other clients
                    msgToSend.strMessage = msgReceived.strName;
                    clientInfo.buffer = new byte[TFMS_Constants.BUFFER_SIZE]; // dimension the clients buffer.
                    CI = clientInfo; // CI would not have been found before since its not in the clientList so just add it now

                    #endregion
                    break;
                case TFMS_Command.Logout:   // client logging out
                    #region Logout command

                    Console.WriteLine("Received Logout from {0}", getNamefromSocket(clientSocket));

                    // tell the server a logout was requested
                    logoffRequested(msgReceived);

                    // remove the client from the list of connected clients.
                    clientList.RemoveAt(findIndexFromClient(clientList, clientSocket));
                    clientSocket.Close();

                    // pass the client's name to other clients
                    msgToSend.strMessage = msgReceived.strName;

                    #endregion
                    break;
                case TFMS_Command.List:     // client requesting a list of all clients
                    #region List command

                    Console.WriteLine("Received List request from {0}", getNamefromSocket(clientSocket));

                    // tell the server a list of clients was requested
                    listRequested(msgReceived);

                    // get the list of all currently connected clients
                    msgToSend = GetClientListMessage();

                    // send the message to a single client
                    sendTFMSmsg(msgToSend, clientSocket, new AsyncCallback(OnSend));

                    #endregion
                    break;
                case TFMS_Command.Message:  // incoming message to be broadcast
                    #region Message command

                    Console.WriteLine("Received TFM from {0} (size={1})", getNamefromSocket(clientSocket), CI.buffer.Length);

                    // tell the server a message broadcast was requested
                    relayRequested(msgReceived);

                    // copy the message to the server's outgoing message field
                    msgToSend.strMessage = msgReceived.strMessage;

                    #endregion
                    break;
                case TFMS_Command.MsgLen:   // incoming message is greater than the current buffer size
                    #region Long Message command

                    Console.WriteLine("Received long TFM from {0} (size={1})", getNamefromSocket(clientSocket), CI.buffer.Length);

                    // resize the buffer for the incoming large message
                    CI.buffer = new byte[int.Parse(msgReceived.strMessage) + TFMS_Constants.BUFFER_SIZE];

                    #endregion
                    break;
                default:                    // unrecognizable command
                    #region Unknown command

                    // we gracefully fail and gtfoh 
                    Console.WriteLine("Cant interpret command! Aborting this relay.");
                    Console.WriteLine("BeginReceive from {0}", getNamefromSocket(clientSocket));

                    // restart the receive command
                    clientSocket.BeginReceive(CI.buffer, 0, CI.buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);

                    return;
                    #endregion
            }
            #endregion


            // send the message to every client logged in to the server
            broadcastToClients(msgToSend, clientList);

            // after a message is broadcast, begin listening for more incoming messages
            // NOTE: if many users are logged in at once, the broadcast method could block other incoming messages
            // for future iterations of TFMS, we want the receiving command to run on a separate thread from the sending command
            if (msgReceived.cmdCommand != TFMS_Command.Logout)
            {
                Console.WriteLine("Waiting for data from {0}", getNamefromSocket(clientSocket));
                clientSocket.BeginReceive(CI.buffer, 0, CI.buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
            }
        }

        /// <summary>
        /// When a message is sent, set the client's state to sent, and end the current send
        /// </summary>
        /// <param name="result">the current socket with updated information</param>
        public void OnSend(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            client.EndSend(result);
        }

        #endregion

        #region TFMS_Server private methods

        /// <summary>
        /// Step through the list of clients, sending the message to everybody currently logged in to the server
        /// This is the "meat and potatoes" of TFMS
        /// </summary>
        /// <param name="data">message to be broadcast</param>
        /// <param name="list">list of clients logged in to the server</param>
        private void broadcastToClients(TFMS_Data data, List<TFMS_ClientInfo> list)
        {
            // List commands are only sent to one recipient
            // MsgLen commands are also not resent to anyone
            if (data.cmdCommand != TFMS_Command.List && data.cmdCommand != TFMS_Command.MsgLen)
            {
                // loop through the client list, sending the message to every client currently logged in
                foreach (TFMS_ClientInfo c in list)
                {
                    // send a message to a single client
                    Console.WriteLine("Relaying {0} from {1} to {2}", data.cmdCommand, data.strName, c.strName);
                    sendTFMSmsg(data, c.socket, new AsyncCallback(OnSend));
                }
            }
        }

        /// <summary>
        /// When the server receives a message from a client, it rebroadcasts the message to all other clients
        /// This method sends the message to a single client
        /// </summary>
        /// <param name="data">TFMS_Data object to be sent</param>
        /// <param name="socket">Socket belonging to the target client</param>
        /// <param name="send">function to be called when the message is done sending (should be "onSend")</param>
        /// <returns>the result of sending the message to the target client</returns>
        private IAsyncResult sendTFMSmsg(TFMS_Data data, Socket socket, AsyncCallback send)
        {
            // get the byte array of the TFMS_Data object
            byte[] message = data.ToByte();

            Console.WriteLine("Sending length: {0}", message.Length);

            // syncronously send the length of the actual message and block until the message has been sent.
            socket.Send(new TFMS_Data(TFMS_Command.MsgLen, string.Format("{0}", message.Length), data.strName).ToByte());

            Console.WriteLine("BeginSend: {0} msg", data.cmdCommand);

            // the async call to send the message
            // returns an IAsyncResult object
            return socket.BeginSend(message, 0, message.Length, SocketFlags.None, send, socket);
        }

        /// <summary>
        /// Take the incoming data from a client and convert it into a TFMS_Data object
        /// </summary>
        /// <param name="socket">the socket that connects the server and the client</param>
        /// <param name="info">the information about the client from the socket</param>
        /// <param name="result">the result of the connection attempt</param>
        /// <returns></returns>
        private TFMS_Data extractData(Socket socket, TFMS_ClientInfo info, IAsyncResult result)
        {
            try
            {
                int numBytesReceived = socket.EndReceive(result); // EndReceive returns how many bytes were received
                byte[] temp = new byte[numBytesReceived];

                // the buffer might not be used for the first Received message so check if it exists
                // otherwise use the default buffer
                if (info == null || info.buffer == null)
                    Array.Copy(byteData, temp, numBytesReceived);
                else
                    Array.Copy(info.buffer, temp, numBytesReceived);

                // parse the bytes and turn it into a TFMS_Data object
                return new TFMS_Data(temp);
            }
            catch (SocketException)
            {
                // if something goes wrong, logoff the client
                return new TFMS_Data(TFMS_Command.Logout, null, getNamefromSocket(socket));
            }
        }

        /// <summary>
        /// Create a TFMS_Data object containing the name of all clients currently connected to the server
        /// </summary>
        /// <returns>TFMS_Data object with message field filled</returns>
        private TFMS_Data GetClientListMessage()
        {
            TFMS_Data msgToSend = new TFMS_Data(TFMS_Command.List, null, null);
            foreach (TFMS_ClientInfo c in clientList)
            {
                // use a * as delimiter
                msgToSend.strMessage += c.strName + "*";
            }

            return msgToSend;
        }

        /// <summary>
        /// Attempts to get the name of the client associated with this socket
        /// </summary>
        /// <param name="socket">active socket connected to a client</param>
        /// <returns>string containing client name (null string otherwise)</returns>
        private string getNamefromSocket(Socket socket)
        {
            int index = findIndexFromClient(this.getClientList(), socket);
            if (index >= 0 && index < this.clientList.Count)
                return this.clientList[index].strName;
            return "";
        }

        /// <summary>
        /// Attempts to find the index of the socket's client name in the list of clients
        /// </summary>
        /// <param name="list">the list of TFMS_ClientInfo objects to search in</param>
        /// <param name="socket">socket who's name we are searching for</param>
        /// <returns>index of the client's name in the TFMS_ClientInfo list, -1 if it is not in the list</returns>
        private int findIndexFromClient(List<TFMS_ClientInfo> list, Socket socket)
        {
            // simple linear search. runs in O(n) time
            int index = 0;
            foreach (TFMS_ClientInfo client in list)
            {
                if (client.socket == socket)
                    return index;
                index++;
            }
            return -1;
        }

        #endregion
    }
}

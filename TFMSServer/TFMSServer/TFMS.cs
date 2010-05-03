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
 * Adapted from: "A Chat Application using Asynchronous TCP Sockets"    *
 * Author: Hitesh Sharma                                                *
 * URL: http://www.codeproject.com/KB/IP/ChatAsynchTCPSockets.aspx      *
 ************************************************************************/

/// <summary>
/// Class definitions:
/// - TFMS_Constants
/// - TFMS_ClientInfo
/// - TFMS_Server
/// - TFMS_Client
/// - TFMS_Data
/// 
/// Delegate definitions:
/// - TFMS_MessageReceived
/// 
/// Enumeration definitions:
/// - TFMS_Command
/// </summary>
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

    /// <summary>
    /// Defines the delegate that handles different types of messages
    /// </summary>
    /// <param name="dataReceived">data from the received message</param>
    public delegate void TFMS_MessageRecieved(TFMS_Data dataReceived);

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
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any,listenPort);
                serverSocket.Bind(ipEndPoint);

                // begin listening for connecting clients
                serverSocket.Listen(5);
                serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

                // if the server is successfully started, print a confirmation and return true
                Console.WriteLine("Begin accepting client connections:");
                return true;
            }
            catch(Exception)
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
                    CI.buffer = new byte[int.Parse(msgReceived.strMessage)+TFMS_Constants.BUFFER_SIZE];

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

                    case TFMS_Command.List:
                        listReceived(msgReceived);
                        break;
                    case TFMS_Command.MsgLen:
                        // this is the only place that a long message has to do work in the client class
                        // everything else is passed on to the client
                        buffSize = int.Parse(msgReceived.strMessage);
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

    /// <summary>
    /// The data structure by which TFMS_Server and TFMS_Client relay messages to each other
    /// </summary>
    [Serializable]
    public class TFMS_Data
    {
        #region TFMS_Data class variables

        /// <summary>
        /// String corresponding to the name of the user who sent the message
        /// </summary>
        public string strName;      

        /// <summary>
        /// The actual data in the message
        /// </summary>
        public string strMessage;   

        /// <summary>
        /// The command field that can be interpreted by the client or receiver and varies depeding on need
        /// </summary>
        public TFMS_Command cmdCommand;  

        /// <summary>
        /// If a user looks at a message in the GUI, it is flagged as acknowledged
        /// </summary>
        public bool acknowledged;

        /// <summary>
        /// Time stamp to show when the message was sent
        /// </summary>
        public DateTime timeStamp;

        #endregion

        #region TFMS_Data constructors

        /// <summary>
        /// Default constructor: creates a new TFMS_Data object with null fields
        /// </summary>
        public TFMS_Data()
            : this(TFMS_Command.Null,null,null) { }

        /// <summary>
        /// Creates a new TFMS_Data object that contains a command, a message, and a user name, all in separate fields
        /// encapsulates all the data about a message into one object
        /// </summary>
        /// <param name="cmd">the message command</param>
        /// <param name="msg">the data portion</param>
        /// <param name="name">the user name of the sender</param>
        public TFMS_Data(TFMS_Command cmd, string msg, string name)
        {
            cmdCommand = cmd;
            strMessage = msg;
            strName = name;
            acknowledged = false;
            timeStamp = DateTime.Now;
        }

        /// <summary>
        /// Creates a new TFMS_Data object by interpreting an array of bytes to fill the fields
        /// </summary>
        /// <param name="data">the byte array to be deserialized</param>
        public TFMS_Data(byte[] data)
        {
            TFMS_Data temp;

            try
            {
                // read the xml data into a generic object
                using (MemoryStream ms = new MemoryStream(data)) // using actually converts into a try/catch that disposes of ms at the end
                {
                    XmlSerializer xser = new XmlSerializer(typeof(TFMS_Data));
                    string tempstr = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

                    // cast the data into a TFMS_Data object
                    temp = (TFMS_Data)xser.Deserialize(ms);
                }
                
            }
            catch (Exception)
            {
                this.clone(new TFMS_Data(TFMS_Command.Null, "", "oops"));
                return;
            }

            // create a new TFMS_Data object by copying the temporary TFMS_Data object
            this.clone(temp);
        }

        #endregion

        #region TFMS_Data public methods

        /// <summary>
        ///  Convert the TFMS_Data object into an array of bytes 
        /// </summary>
        /// <returns>the byte array representing the XML serialized data object</returns>
        public byte[] ToByte()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xser = new XmlSerializer(typeof(TFMS_Data));
                xser.Serialize(ms, this);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// Received messages are displayed green for unacknowledged (unread) messages and red for acknowledged (read) messages
        /// </summary>
        public Brush dispColor
        {
            get
            {
                if (!acknowledged)
                    return Brushes.Red;
                else
                    return Brushes.Green;
            }
        }

        #endregion

        #region TFMS_Data private methods

        /// <summary>
        /// Performs a shallow copy of the specified TFMS_Data object
        /// </summary>
        /// <param name="data">the TFMS_Data object that you want to duplicate</param>
        private void clone(TFMS_Data data)
        {
            if (data == null) return;
            this.acknowledged = data.acknowledged;
            this.cmdCommand = data.cmdCommand;
            this.strMessage = data.strMessage;
            this.strName = data.strName;
            this.timeStamp = data.timeStamp;
        }

        #endregion

    }

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
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
/// contains class definitions for:
/// - TFMS
/// </summary>
namespace TFMS_Space
{
    /// <summary>
    /// holds list of constants used in both the client and server classes
    /// </summary>
    public class TFMS_Constants
    {
        public const int delayTime = 100;
        public const long buffSize = 1024;
    }

    /// <summary>
    /// defines the delegate that handles different types of messages
    /// </summary>
    /// <param name="dataReceived">data from the received message</param>
    public delegate void TFMS_MessageRecieved(TFMS_Data dataReceived);

    /// <summary>
    /// contains all information about a person connected to the server
    /// </summary>
    public class TFMS_ClientInfo
    {
        /// <summary>
        /// the physical socket used to comunicate with the client
        /// </summary>
        public Socket socket;
        /// <summary>
        /// provides the given moniker/userName of the client
        /// </summary>
        public string strName;
        /// <summary>
        /// dedicated buffer for the recipt of tcp messages
        /// </summary>
        public byte[] buffer;

        /// <summary>
        /// creates a basic Client info based on a connected socket and a name
        /// </summary>
        /// <param name="a">the socket to associate with this client</param>
        /// <param name="b">the name given by the client</param>
        public TFMS_ClientInfo(Socket a, string b)
        {
            socket = a;
            strName = b;
            buffer = null;
        }
    }
    /// <summary>
    /// this encapsulates all the functions of the server module. 
    /// using this class is as simple as:
    /// - declaring an instance
    /// - assigning delegate functions to handle messages
    /// - starting the server.
    /// </summary>
    public class TFMS_Server
    {
        /// <summary>
        /// this holds all the info about connected clients see definition for ClientInfo
        /// </summary>
        public List<TFMS_ClientInfo> clientList;

        /// <summary>
        /// the socket that listens for new clients
        /// </summary>
        Socket serverSocket;

        /// <summary>
        /// this is the buffer for receiving connect requests
        /// </summary>
        byte[] byteData;

        /// <summary>
        /// this is the port that the server listens on for incoming connections
        /// (individual clients get their own port numbers
        /// </summary>
        public int listenPort;

        /// <summary>
        /// 
        /// </summary>
        public TFMS_MessageRecieved logonRequested;
        public TFMS_MessageRecieved logoffRequested;
        public TFMS_MessageRecieved relayRequested;
        public TFMS_MessageRecieved listRequested;


        #region constructors
        /// <summary>
        /// creates a server that will listen on the default port of 1000
        /// TODO: make 1000 into a constant in TFMS_Consts
        /// </summary>
        public TFMS_Server()
            : this(1000)
        { }
        /// <summary>
        /// creates a server that will listen for clients
        /// </summary>
        /// <param name="port">the port to listen on</param>
        public TFMS_Server(int port)
        {
            listenPort = port;
            byteData = new byte[TFMS_Constants.buffSize];
            clientList = new List<TFMS_ClientInfo>();
        }
        #endregion
        #region basic server tasks
        /// <summary>
        /// call this when you have setup all the settings on the server to start accepting clients.
        /// </summary>
        /// <returns>true if the server starts, false if there is an error</returns>
        public bool startServer()
        {
            try 
            {
                serverSocket=new Socket(AddressFamily.InterNetwork,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any,listenPort);
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(5);
                Console.WriteLine("Begin Accept:");
                serverSocket.BeginAccept(new AsyncCallback(OnAccept),null);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
        #endregion
        #region async methods
        /// <summary>
        /// this is called anytime someone tries to connect to the server.
        /// </summary>
        /// <param name="ar">this allows us to get the Socket the client is using to talk to us with</param>
        private void OnAccept(IAsyncResult ar)
        {
            //get the socket the client and server are comunicating through
            Socket clientSocket = serverSocket.EndAccept(ar);

            // start listening for other clients
            serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

            //setup the async receive (receieve is getting actual messages)
            Console.WriteLine("Begin Recieve:");

            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                         new AsyncCallback(OnReceive), clientSocket);
        }

        /// <summary>
        /// this hadles the actual message forwarding ect.
        /// you should never call this function directly
        /// </summary>
        /// <param name="ar">this encapsulates the Socket of the client</param>
        private void OnReceive(IAsyncResult ar)
        {
            while (!ar.IsCompleted) Thread.Sleep(100);//TODO: remove this. its pointless.
            long buffSize = TFMS_Constants.buffSize; // assume we will have the default buffer size
            Socket clientSocket = (Socket)ar.AsyncState;     // get the socket that received data

            TFMS_ClientInfo CI = null;
            #region get ClientInfo based on clientSocket
            // attempt to find the ClientInfo related to the socket
            // if there is no client info CI will be null
            //TODO: implement as a hash HASH<SOCKET,CLIENTINFO>
            
            if (findIndexFromClient(clientList, clientSocket) >= 0)
                CI = clientList[findIndexFromClient(clientList, clientSocket)];
            #endregion

            Console.WriteLine("\n\nData Received from:{0}", getNamefromSocket(clientSocket));

            //attempt to get the data and convert it into data.
            #region get Data message from socket
            TFMS_Data msgReceived;
            try
            {
                int numBytesReceived = clientSocket.EndReceive(ar); // End receive tells you how many bytes were received
                byte[] temp = new byte[numBytesReceived];
                
                // the buffer might not be used for the first Received message so check if it exists.
                // otherwise use the default buffer.
                if (CI == null || CI.buffer == null)
                {
                    Array.Copy(byteData, temp, numBytesReceived);
                }
                else
                {
                    Array.Copy(CI.buffer, temp, numBytesReceived);
                }
                // parse the bytes and turn it into a Data object (this is a big step!)
                msgReceived = new TFMS_Data(temp);
            }
            catch (SocketException)
            {
                //if for any reason shit get messed up boot the client.
                msgReceived = new TFMS_Data(TFMS_Command.Logout, null, getNamefromSocket(clientSocket));
            }
            #endregion

            #region generate message to send to clients
            TFMS_Data msgToSend = new TFMS_Data();
            msgToSend.cmdCommand = msgReceived.cmdCommand; // echo the command
            msgToSend.strName = msgReceived.strName;       // echo the name of the client
            #endregion

            

            //handle the message appropriately this is where you should add code to deal with new features
            //(aknowladge...)

            //TODO: call the new all encompasing Message handler here (not in each case block)
            //messageReceived(Data);
            switch (msgReceived.cmdCommand)
            {
                case TFMS_Command.Login:
                    #region handle Login
                    logonRequested(msgReceived); // tell the user of the class we have received a logon
                    
                    //When a user logs in to the server then we add her to our list of clients                    
                    TFMS_ClientInfo clientInfo = new TFMS_ClientInfo(clientSocket, msgReceived.strName);
                    clientList.Add(clientInfo);

                    Console.WriteLine("received login from:{0}", getNamefromSocket(clientSocket));
                    //just pass the name to other clients for now
                    // let the client program handle how to deal with it.
                    // the command and strName have been filled above.
                    msgToSend.strMessage = msgReceived.strName;
                    clientInfo.buffer = new byte[TFMS_Constants.buffSize]; // dimension the clients buffer.
                    CI = clientInfo; // CI would not have been found before since its not in the clientList so just add it now
                    #endregion
                    break;
                case TFMS_Command.Logout:
                    #region handle Logout
                    logoffRequested(msgReceived); // tell the user of the class we have received a logon
                    Console.WriteLine("Received Logout from:{0}", getNamefromSocket(clientSocket));

                    // remove the client from the list of connected clients.
                    clientList.RemoveAt(findIndexFromClient(clientList, clientSocket));
                    clientSocket.Close();
                    //just pass the name to other clients for now
                    // let the client program handle how to deal with it.
                    // the command and strName have been filled above.
                    msgToSend.strMessage = msgReceived.strName;
                    #endregion
                    break;
                case TFMS_Command.List:
                    #region handle List
                    listRequested(msgReceived); // tell the user of the class we have received a List Request
                    Console.WriteLine("Receved List Rq from:{0}", getNamefromSocket(clientSocket));

                    msgToSend = GetClientListMessage(); // helper function generates the message for us
                    // send to a single person 
                    // since its not being brodcast to the whole chatroom 
                    // we send it here inside the case block
                    sendTFMSmsg(msgToSend, clientSocket, new AsyncCallback(OnSend));
                    buffSize = TFMS_Constants.buffSize;
                    #endregion
                    break;
                case TFMS_Command.Message:
                    #region handle Message
                    relayRequested(msgReceived); // tell the user of the class we have received a Message that must be Relayed
                    Console.WriteLine("Received Message from:{0} size=({1})", getNamefromSocket(clientSocket), CI.buffer.Length);
                    // copy the message to the "message To be Sent"
                    msgToSend.strMessage = msgReceived.strMessage;
                    #endregion
                    break;
                case TFMS_Command.MsgLen:
                    #region handle MsgLen
                    // all we have to do here is resize the buffer for the incoming HUGE message
                    buffSize = int.Parse(msgReceived.strMessage)+TFMS_Constants.buffSize; // padds the size by a constant (probably not neccasarry) remove before release
                    Console.WriteLine("Received MsgLen:{0} from:{1}", buffSize, getNamefromSocket(clientSocket));
                    CI.buffer=new byte[ buffSize];
                    #endregion
                    break;
                default:
                    #region handle unknown cmd
                    // we gracefully fail and gtfoh 
                    Console.WriteLine("cant interpret command! aborting relay.");
                    Console.WriteLine("BeginRecive from :{0}", getNamefromSocket(clientSocket));
                    clientSocket.BeginReceive(CI.buffer, 0, CI.buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                    return;
                    #endregion
            }

            #region rebrodcast message as needed
            //List messages are only sent to one recipient so dont forward them
            //MsgLen messages are not resent to anyone so likewise they are not forwarded to anyone.
            if (msgToSend.cmdCommand != TFMS_Command.List && msgToSend.cmdCommand != TFMS_Command.MsgLen)
            {
                foreach (TFMS_ClientInfo c in clientList)
                {
                    //if (c.socket != clientSocket) //dont send to yourself 
                    //{
                    Console.WriteLine("relaying {0} from {1} to {2}", msgToSend.cmdCommand, msgToSend.strName, c.strName);
                    sendTFMSmsg(msgToSend, c.socket, new AsyncCallback(OnSend)); // helper function to send a message to a single recipient
                    //}
                }
            }
            #endregion
            #region start receving again
            //after dealing with the message we need to keep listening for the NEXT message
            // unless the message was logout.
            if (msgReceived.cmdCommand != TFMS_Command.Logout)
            {
                Console.WriteLine("Waiting for data from :{0} receiveLen={1}", getNamefromSocket(clientSocket),byteData.Length);
                clientSocket.BeginReceive(CI.buffer, 0, CI.buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
            }
            #endregion

        }

        /// <summary>
        /// this method is boilerplate code to deal with asyncronous sending of messages
        /// you should never call this function directly
        /// </summary>
        /// <param name="ar">encapsulates the socket</param>
        public void OnSend(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndSend(ar);
            //Console.WriteLine("EndSend:{0}", client);
        }

        /// <summary>
        /// this will send a message to a single user asyncronously
        /// </summary>
        /// <param name="d">the data message to send</param>
        /// <param name="s">the socket corrisponding to the user you want to send to</param>
        /// <param name="snd">the function that will be called when the send is complete (typicaly "OnSend")</param>
        /// <returns></returns>
        public IAsyncResult sendTFMSmsg(TFMS_Data d, Socket s, AsyncCallback snd)
        {
            byte[] message = d.ToByte(); // messages must be sent as Arrays of bytes
            //if (message.Length >= TFMSConsts.buffSize)
            //{
                Console.WriteLine("sending length: {0}", message.Length);
                // syncronously send the length of the actual message and block until the message has been sent.
                s.Send(new TFMS_Data(TFMS_Command.MsgLen, string.Format("{0}", message.Length), d.strName).ToByte()); 
                Thread.Sleep(TFMS_Constants.delayTime); // saftey delay, should be removed before release
            //}
            Console.WriteLine("BeginSend:{0} msg",d.cmdCommand);
            return s.BeginSend(message, 0, message.Length, SocketFlags.None, snd, s); // the async call to Send the message
        }
        #endregion
        #region helper functions
        /// <summary>
        /// create a Data object the has a message with the name of each client that is connected to the server.
        /// Currently it is one string with "*" delimited names
        /// </summary>
        /// <returns>a Data object with strMessage filled and Cmd=List but no name attached.</returns>
        public TFMS_Data GetClientListMessage()
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
        /// gets the index into the lst where the ClientInfo.Socket == s
        /// </summary>
        /// <param name="lst">the List of ClientInfo to search</param>
        /// <param name="s">the socket you are searching for</param>
        /// <returns>if lst contains a client with socket ==s the index of that client
        /// else -1</returns>
        public int findIndexFromClient(List<TFMS_ClientInfo> lst, Socket s)
        {
            // simple linear search. runs in O(n) time
            int nIndex = 0;
            foreach (TFMS_ClientInfo client in lst)
            {
                if (client.socket == s)
                    return nIndex;
                ++nIndex;
            }
            return -1;
        }
        
        /// <summary>
        /// trys to get a userName associated with a socket
        /// </summary>
        /// <param name="s">the socket to retrieve a name for</param>
        /// <returns>s exists: a string
        /// else a blank string</returns>
        public string getNamefromSocket(Socket s)
        {
            int index = findIndexFromClient(this.clientList, s);
            if (index >= 0 && index < this.clientList.Count)
                return this.clientList[index].strName;
            return "";
        }
        #endregion
    }

    /// <summary>
    /// this class encapsulates all the background stuff for a client program.
    /// </summary>
    public class TFMS_Client
    {
        /// <summary>
        /// the port to try to contact the server on.
        /// </summary>
        public int serverPort;
        /// <summary>
        /// this is the socket for comunicating with the server.
        /// </summary>
        public Socket clientSocket;
        /// <summary>
        /// the username you have given yourself
        /// </summary>
        public string strName;
        /// <summary>
        /// testing variable to examine the last message sent. remove before release.
        /// </summary>
        public string lastMessage;
        /// <summary>
        /// buffer for data to be Received from the server.
        /// </summary>
        byte[] byteData;

        #region event delegates
        //TODO: only implement one delegate that will handle all message types including ones yet to be implemented.
        public event TFMS_MessageRecieved loginReceived;
        public event TFMS_MessageRecieved logoffReceived;
        public event TFMS_MessageRecieved dataReceived;
        public event TFMS_MessageRecieved listReceived;
        public event TFMS_MessageRecieved disconnectDetected;
        #endregion
        #region contructors
        /// <summary>
        /// default constructor with bogus values.
        /// you should never call this directly
        /// </summary>
        public TFMS_Client()
            :this(1000,"Jon Doe")
        {}
        /// <summary>
        /// creates a client that will try to connect to the server on a given port with a userName
        /// </summary>
        /// <param name="port">the port to initiate contact with the server on. (0-65535)</param>
        /// <param name="Name">the name you want to use to comunicate</param>
        public TFMS_Client(int port, string Name)
        {
            serverPort = port;
            clientSocket = null;
            strName = Name;
            byteData = new byte[TFMS_Constants.buffSize];
        }
        #endregion
        #region basic command tasks
        /// <summary>
        /// connect to the server
        /// </summary>
        /// <param name="ipaddr">the ip address string ex "127.0.0.1"</param>
        /// <returns>true if the connection was succesful. otherwise false</returns>
        public bool connect(string ipaddr) 
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress serverip=IPAddress.Parse(ipaddr);
                IPEndPoint ipEnd = new IPEndPoint(serverip,serverPort);
                
                //the following is blocking calls (abstract out into a background worker)
                clientSocket.Connect(ipEnd);

                //immediately try to login after connect
                TFMS_Data msgToSend = new TFMS_Data(TFMS_Command.Login, null, strName);
                lastMessage= msgToSend.ToString();
                clientSocket.Send(msgToSend.ToByte());

                //setup buffer to receive data from the server.
                byteData = new byte[TFMS_Constants.buffSize];

                //Start listening to the data asynchronously
                clientSocket.BeginReceive(byteData,
                                       0, 
                                       byteData.Length,
                                       SocketFlags.None,
                                       new AsyncCallback(OnReceive),
                                       null);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// disconnect from the server.
        /// sends a logout message to the server.
        /// </summary>
        public void disconnect()
        {
            TFMS_Data msgToSend = new TFMS_Data(TFMS_Command.Logout, null, strName);
            byte[] data = msgToSend.ToByte();
            lastMessage= msgToSend.ToString();
            clientSocket.Send(data);
        }

        /// <summary>
        /// requests a list of users connected to the server.
        /// </summary>
        public void getList()
        {
            TFMS_Data msgToSend = new TFMS_Data(TFMS_Command.List,null,strName);
            byte[] data = msgToSend.ToByte();
            //if (data.Length >= TFMSConsts.buffSize)
            //{
                TFMS_Data lenToSend = new TFMS_Data(TFMS_Command.MsgLen, string.Format("{0}", data.Length), strName);
                lastMessage= lenToSend.ToString();
                clientSocket.Send(lenToSend.ToByte());
                Thread.Sleep(TFMS_Constants.delayTime );
            //}
            lastMessage= msgToSend.ToString();
            clientSocket.BeginSend(data,0,data.Length,SocketFlags.None,new AsyncCallback(OnSend),null);
        }

        /// <summary>
        /// helper function that sends a Message that needs to be relayed to other clients to the server.
        /// </summary>
        /// <param name="data">the string that will be relayed to other clients</param>
        public void sendMessage(string data)
        {
            TFMS_Data msgToSend = new TFMS_Data(TFMS_Command.Message, data, strName);
            byte[] d = msgToSend.ToByte();
            lastMessage=msgToSend.ToString();
            //if (d.Length >= TFMSConsts.buffSize)
            //{
                TFMS_Data lenToSend = new TFMS_Data(TFMS_Command.MsgLen, string.Format("{0}", d.Length), strName);
                lastMessage= lenToSend.ToString();
                clientSocket.Send(lenToSend.ToByte());
                Thread.Sleep(TFMS_Constants.delayTime);
            //}
            lastMessage= msgToSend.ToString();
            clientSocket.BeginSend(d, 0, d.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
        }
        #endregion
        #region async methods

        /// <summary>
        /// boiler plate code for sending async messages
        /// never call this explicitly the .NET framework will call it.
        /// </summary>
        /// <param name="ar"></param>
        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("this object has been disposed.");
            }
        }

        /// <summary>
        /// method to deal with messages received from the server.
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            long bufSize = TFMS_Constants.buffSize;// assume default buffer size
            try
            {
                int numBytesReceived = 0;
                TFMS_Data msgReceived;
                #region get size of received message (also detects if server blew up)
                
                
                try
                {
                    numBytesReceived = clientSocket.EndReceive(ar);
                }
                catch (Exception)
                {
                    disconnectDetected(null);
                    return;
                }
                #endregion

                
                #region parse bytes into a Data object
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
                #endregion
                //Accordingly process the message received
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
                        // this is the only place that has to do work in the client class.
                        // everything else is passed on to the programmer using the class
                        bufSize = int.Parse(msgReceived.strMessage);
                        break;
                    case TFMS_Command.Null:
                        break;
                }

                byteData = new byte[bufSize];


                // wait for next message from server.
                clientSocket.BeginReceive(byteData,
                                          0,
                                          byteData.Length,
                                          SocketFlags.None,
                                          new AsyncCallback(OnReceive),
                                          null);

            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("this object has been disposed");
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                    this.clientSocket = null;

            }
            
            
        }
        #endregion
    }

    /// <summary>
    /// The data structure by which the server and the client interact with each other
    /// </summary>
    [Serializable]
    public class TFMS_Data
    {
        /// <summary>
        /// the string corrisponding to the moniker of the user who sent the message
        /// </summary>
        public string strName;      
        /// <summary>
        /// message text ( this can be just about anything)
        /// </summary>
        public string strMessage;   
        /// <summary>
        /// the enumeration of different things you can do
        /// </summary>
        public TFMS_Command cmdCommand;  
        /// <summary>
        /// this tells the client if you have acknowledged this message
        /// </summary>
        public bool acknowledged;
        /// <summary>
        /// the time the message was created
        /// </summary>
        public DateTime timeStamp;

        /// <summary>
        /// default Constructor fills the fields with null data
        /// </summary>
        public TFMS_Data()
            :this(TFMS_Command.Null,null,null)
        { }

        /// <summary>
        /// encapsulates all the data about a message into one object
        /// </summary>
        /// <param name="c">the command type</param>
        /// <param name="msg">the actual message string</param>
        /// <param name="name">the moniker of the sender</param>
        public TFMS_Data(TFMS_Command c,string msg,string name)
        {
            cmdCommand = c; strMessage = msg; strName = name; acknowledged = false;
            timeStamp = DateTime.Now;
        }

        
        
        /// <summary>
        /// constructor that deserializes an array of bytes formated as xml
        /// </summary>
        /// <param name="data">the byte buffer to be deserialized</param>
        public TFMS_Data(byte[] data)
        {
            TFMS_Data temp;
            try
            {
                using (MemoryStream ms = new MemoryStream(data)) // using actually converts into a try/catch that disposes of ms at the end
                {
                    XmlSerializer xser = new XmlSerializer(typeof(TFMS_Data));
                    string tempstr = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                    temp = (TFMS_Data)xser.Deserialize(ms);
                }
                
            }
            catch (Exception)
            {
                this.Clone(new TFMS_Data(TFMS_Command.Null, "", "oops"));
                return ;
            }
            this.Clone(temp);
        }


        /// <summary>
        /// does a shallow copy from "d" to "this"
        /// </summary>
        /// <param name="d">the Data you want to duplicate</param>
        public void Clone(TFMS_Data d)
        {
            if (d == null) return;
            this.acknowledged = d.acknowledged;
            this.cmdCommand = d.cmdCommand;
            this.strMessage = d.strMessage;
            this.strName = d.strName;
            this.timeStamp = d.timeStamp;
        }

        /// <summary>
        ///  Converts the Data structure into an array of bytes 
        /// </summary>
        /// <returns>the byte array that represents the XML serialized data object</returns>
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
        /// the full XML representation of the object
        /// </summary>
        /// <returns>the full XML representation of the object</returns>
        public override  string ToString()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xser = new XmlSerializer(typeof(TFMS_Data));

                xser.Serialize(ms, this);
                return System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
            }
        }

        /// <summary>
        /// the data knows what color it should be in the list box.
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
    }

    /// <summary>
    /// The commands for interaction between the server and the client
    /// </summary>
    public enum TFMS_Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        Null,
        MsgLen,        //No command
    }

}

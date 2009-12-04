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

/*/
 * credit to:A Chat Application Using Asynchronous TCP Sockets by Hitesh Sharma for orignal implementation
 * code has now been reworked into a more modular format for general use.
/*/


namespace TFMS_Space
{
    public class TFMSConsts
    {
        public const int delayTime = 100;
        public const long buffSize = 1024;
        public const bool useXML = false;
    }
    public delegate void MessageRecieved(Data dataReceived);
    public class ClientInfo
    {
        public Socket socket;
        public string strName;
        public byte[] buffer;

        public ClientInfo(Socket a, string b)
        {
            socket = a;
            strName = b;
            buffer = null;
        }
    }
    public class TFMSServer
    {
        public List<ClientInfo> clientList;
        Socket serverSocket; // main socket to listen to;
        byte[] byteData;
        public int listenPort;

        #region event delegates
        public MessageRecieved logonRequested;
        public MessageRecieved logoffRequested;
        public MessageRecieved RelayRequested;
        public MessageRecieved ListRequested;
        #endregion
        #region constructors
        public TFMSServer()
            : this(1000)
        { }
        public TFMSServer(int port)
        {
            listenPort = port;
            byteData = new byte[TFMSConsts.buffSize];
            clientList = new List<ClientInfo>();
        }
        #endregion
        #region basic server tasks
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
            catch(Exception ex)
            {
                return false;
            }
        }
        #endregion
        #region async methods
        private void OnAccept(IAsyncResult ar)
        {
            //get the socket the client and server are comunicating through
            Socket clientSocket = serverSocket.EndAccept(ar);

            // start listening for other clients
            serverSocket.BeginAccept(new AsyncCallback(OnAccept), null);

            //setup the async receive
            Console.WriteLine("Begin Recieve:");

            clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                         new AsyncCallback(OnReceive), clientSocket);
        }
        private void OnReceive(IAsyncResult ar)
        {
            while (!ar.IsCompleted) Thread.Sleep(100);
            long buffSize = TFMSConsts.buffSize;
            Socket clientSocket = (Socket)ar.AsyncState;     // get the socket that received data


            #region get ClientInfo based on clientSocket
            // attempt to find the ClientInfo related to the socket
            // if there is no client info CI will be null
            //TODO: implement as a hash
            ClientInfo CI=null;
            if (findIndexFromClient(clientList, clientSocket) >= 0)
                CI = clientList[findIndexFromClient(clientList, clientSocket)];
            #endregion

            Console.WriteLine("\n\nData Received from:{0}", getNamefromSocket(clientSocket));

            //attempt to get the data and convert it into data.
            #region get Data message from socket
            Data msgReceived;
            try
            {
                int numBytesReceived = clientSocket.EndReceive(ar);
                byte[] temp = new byte[numBytesReceived];
                if (CI == null || CI.buffer == null)
                {
                    Array.Copy(byteData, temp, numBytesReceived);
                }
                else
                {
                    Array.Copy(CI.buffer, temp, numBytesReceived);
                }
                msgReceived = new Data(temp);
            }
            catch (SocketException ex)
            {
                msgReceived = new Data(Command.Logout, null, getNamefromSocket(clientSocket));

            }
            #endregion

            #region generate message to send to clients
            Data msgToSend = new Data();
            msgToSend.cmdCommand = msgReceived.cmdCommand;
            msgToSend.strName = msgReceived.strName;
            #endregion

            switch (msgReceived.cmdCommand)
            {
                case Command.Login:
                    #region handle Login
                    //When a user logs in to the server then we add her to our list of clients
                    logonRequested(msgReceived);
                    ClientInfo clientInfo = new ClientInfo(clientSocket, msgReceived.strName);
                    clientList.Add(clientInfo);
                    Console.WriteLine("received login from:{0}", getNamefromSocket(clientSocket));
                    //just pass the name to other clients for now
                    // let the client program handle how to deal with it.
                    msgToSend.strMessage = msgReceived.strName;
                    clientInfo.buffer = new byte[TFMSConsts.buffSize];
                    CI = clientInfo; // i hope this is a reference assignment
                    //buffSize = TFMSConsts.buffSize;
                    //byteData = new byte[buffSize];
                    #endregion
                    break;
                case Command.Logout:
                    #region handle Logout
                    Console.WriteLine("Received Logout from:{0}", getNamefromSocket(clientSocket));
                    logoffRequested(msgReceived);
                    //find the index of the client whose socket we are provided
                    clientList.RemoveAt(findIndexFromClient(clientList, clientSocket));
                    //clientSocket.EndReceive(ar);
                    //Console.WriteLine("EndReceive:");
                    clientSocket.Close();
                    msgToSend.strMessage = msgReceived.strName;
                    #endregion
                    break;
                case Command.List:
                    #region handle List
                    Console.WriteLine("Receved List Rqfrom:{0}", getNamefromSocket(clientSocket));
                    ListRequested(msgReceived);
                    msgToSend = GetClientListMessage();
                    //send to a single person
                    sendTFMSmsg(msgToSend, clientSocket, new AsyncCallback(OnSend));
                    buffSize = TFMSConsts.buffSize;
                    #endregion
                    break;
                case Command.Message:
                    #region handle Message
                    Console.WriteLine("Received Message from:{0} size=({1})", getNamefromSocket(clientSocket), CI.buffer.Length);
                    RelayRequested(msgReceived);
                    msgToSend.strMessage = msgReceived.strMessage;
                    #endregion
                    break;
                case Command.MsgLen:
                    #region handle MsgLen
                    buffSize = int.Parse(msgReceived.strMessage)+TFMSConsts.buffSize;
                    Console.WriteLine("Received MsgLen:{0} from:{1}", buffSize, getNamefromSocket(clientSocket));
                    CI.buffer=new byte[ buffSize];
                    //byteData = new byte[buffSize];
                    #endregion
                    break;
                default:
                    #region handle unknown cmd
                    Console.WriteLine("cant interpret command! aborting relay.");
                    Console.WriteLine("BeginRecive from :{0}", getNamefromSocket(clientSocket));
                    clientSocket.BeginReceive(CI.buffer, 0, CI.buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                    return;
                    #endregion
            }

            #region rebrodcast message as needed
            //broadcast messages if needed
            if (msgToSend.cmdCommand != Command.List && msgToSend.cmdCommand != Command.MsgLen)
            {
                foreach (ClientInfo c in clientList)
                {
                    //if (c.socket != clientSocket) //dont send to yourself
                    //{
                    Console.WriteLine("relaying {0} from {1} to {2}", msgToSend.cmdCommand, msgToSend.strName, c.strName);
                    sendTFMSmsg(msgToSend, c.socket, new AsyncCallback(OnSend));
                    //}
                }
            }
            #endregion
            #region start receving again
            //after dealing with the message we need to keep listening
            // unless the message was "peace out!"
            if (msgReceived.cmdCommand != Command.Logout)
            {
                Console.WriteLine("Waiting for data from :{0} receiveLen={1}", getNamefromSocket(clientSocket),byteData.Length);
                clientSocket.BeginReceive(CI.buffer, 0, CI.buffer.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
            }
            #endregion

        }
        public void OnSend(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndSend(ar);
            //Console.WriteLine("EndSend:{0}", client);
        }
        public IAsyncResult sendTFMSmsg(Data d, Socket s, AsyncCallback snd)
        {
            byte[] message = d.ToByte();
            //if (message.Length >= TFMSConsts.buffSize)
            //{
                Console.WriteLine("sending length: {0}", message.Length);
                s.Send(new Data(Command.MsgLen, string.Format("{0}", message.Length), d.strName).ToByte());
                Thread.Sleep(TFMSConsts.delayTime);
            //}
            Console.WriteLine("BeginSend:{0} msg",d.cmdCommand);
            return s.BeginSend(message, 0, message.Length, SocketFlags.None, snd, s);
        }
        #endregion
        #region helper functions
        public Data GetClientListMessage()
        {
            Data msgToSend = new Data(Command.List, null, null);
            foreach (ClientInfo c in clientList)
            {
                // use a * as delimiter
                msgToSend.strMessage += c.strName + "*";
            }
            return msgToSend;
        }
        public int findIndexFromClient(List<ClientInfo> lst, Socket s)
        {
            int nIndex = 0;
            foreach (ClientInfo client in lst)
            {
                if (client.socket == s)
                    return nIndex;
                ++nIndex;
            }
            return -1;
        }
        public string getNamefromSocket(Socket s)
        {
            int index = findIndexFromClient(this.clientList, s);
            if (index >= 0 && index < this.clientList.Count)
                return this.clientList[index].strName;
            return "";
        }
        #endregion
    }
    public class TFMSClient
    {
        public int serverPort;
        public Socket clientSocket;
        public string strName;
        public string lastMessage;
        byte[] byteData;

        #region event delegates
        public event MessageRecieved loginReceived;
        public event MessageRecieved logoffReceived;
        public event MessageRecieved dataReceived;
        public event MessageRecieved listReceived;
        public event MessageRecieved disconnectDetected;
        #endregion
        #region contructors
        public TFMSClient()
            :this(1000,"Jon Doe")
        {}
        public TFMSClient(int port, string Name)
        {
            serverPort = port;
            clientSocket = null;
            strName = Name;
            byteData = new byte[TFMSConsts.buffSize];
        }
        #endregion
        #region basic command tasks
        public bool connect(string ipaddr) // connect to server
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress serverip=IPAddress.Parse(ipaddr);
                IPEndPoint ipEnd = new IPEndPoint(serverip,serverPort);
                //the following is blocking calls (abstract out into a background worker)
                
                clientSocket.Connect(ipEnd);
                Data msgToSend = new Data(Command.Login, null, strName);
                lastMessage= msgToSend.ToString();
                clientSocket.Send(msgToSend.ToByte());

                byteData = new byte[TFMSConsts.buffSize];
                //Start listening to the data asynchronously
                clientSocket.BeginReceive(byteData,
                                       0, 
                                       byteData.Length,
                                       SocketFlags.None,
                                       new AsyncCallback(OnReceive),
                                       null);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void disconnect()
        {
            Data msgToSend = new Data(Command.Logout, null, strName);
            byte[] data = msgToSend.ToByte();
            lastMessage= msgToSend.ToString();
            //Data lenToSend = new Data(Command.MsgLen, string.Format("{0}", data.Length), strName);
            //clientSocket.Send(lenToSend.ToByte());
            clientSocket.Send(data);
        }

        public void getList()
        {
            Data msgToSend = new Data(Command.List,null,strName);
            byte[] data = msgToSend.ToByte();
            //if (data.Length >= TFMSConsts.buffSize)
            //{
                Data lenToSend = new Data(Command.MsgLen, string.Format("{0}", data.Length), strName);
                lastMessage= lenToSend.ToString();
                clientSocket.Send(lenToSend.ToByte());
                Thread.Sleep(TFMSConsts.delayTime );
            //}
            lastMessage= msgToSend.ToString();
            clientSocket.BeginSend(data,0,data.Length,SocketFlags.None,new AsyncCallback(OnSend),null);
        }

        public void sendMessage(string data)
        {
            Data msgToSend = new Data(Command.Message, data, strName);
            byte[] d = msgToSend.ToByte();
            lastMessage=msgToSend.ToString();
            //if (d.Length >= TFMSConsts.buffSize)
            //{
                Data lenToSend = new Data(Command.MsgLen, string.Format("{0}", d.Length), strName);
                lastMessage= lenToSend.ToString();
                clientSocket.Send(lenToSend.ToByte());
                Thread.Sleep(TFMSConsts.delayTime);
            //}
            lastMessage= msgToSend.ToString();
            clientSocket.BeginSend(d, 0, d.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
        }
        #endregion
        #region async methods
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

        private void OnReceive(IAsyncResult ar)
        {
            long bufSize = TFMSConsts.buffSize;
            try
            {
                int numBytesReceived=0;
                try
                {
                    numBytesReceived = clientSocket.EndReceive(ar);
                }
                catch (Exception ex)
                {
                    disconnectDetected(null);
                    return;
                }
                Data msgReceived;
                if (numBytesReceived == 0)
                {
                    msgReceived = new Data(Command.Null, "", "");
                }
                else
                {
                    byte[] temp = new byte[numBytesReceived];
                    Array.Copy(byteData, temp, numBytesReceived);
                    msgReceived = new Data(temp);
                }
                //Accordingly process the message received
                switch (msgReceived.cmdCommand)
                {
                    case Command.Login:
                        loginReceived(msgReceived);
                        break;

                    case Command.Logout:
                        logoffReceived(msgReceived);
                        break;

                    case Command.Message:
                        dataReceived(msgReceived);
                        break;

                    case Command.List:
                        listReceived(msgReceived);
                        break;
                    case Command.MsgLen: // this will let us use HUGE messages
                        bufSize = int.Parse(msgReceived.strMessage);
                        break;
                    case Command.Null:
                        break;
                }

                byteData = new byte[bufSize];

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

    //The data structure by which the server and the client interact with 
    //each other
    [Serializable]
    public class Data
    {
        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
        public bool acknowledged;
        public DateTime timeStamp;

        //Default constructor
        public Data()
            :this(Command.Null,null,null)
        { }

        public Data(Command c,string msg,string name)
        {
            cmdCommand = c; strMessage = msg; strName = name; acknowledged = false;
            timeStamp = DateTime.Now;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            Data temp;
            using (MemoryStream ms = new MemoryStream(data))
            {
                XmlSerializer xser = new XmlSerializer(typeof(Data));
                string tempstr = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                temp = (Data)xser.Deserialize(ms);
            }
            this.Clone(temp);
        }

        public void Clone(Data d)
        {
            if (d == null) return;
            this.acknowledged = d.acknowledged;
            this.cmdCommand = d.cmdCommand;
            this.strMessage = d.strMessage;
            this.strName = d.strName;
            this.timeStamp = d.timeStamp;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xser = new XmlSerializer(typeof(Data));
                xser.Serialize(ms, this);
                ms.Seek(0, SeekOrigin.Begin); // TODO: i think this can be removed but im to scared to do it atm (12-04-2009)
                return ms.GetBuffer();
            }
        }
        public override  string ToString()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xser = new XmlSerializer(typeof(Data));

                xser.Serialize(ms, this);
                return System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
            }
        }

        
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

    //The commands for interaction between the server and the client
    public enum Command
    {
        Login,      //Log into the server
        Logout,     //Logout of the server
        Message,    //Send a text message to all the chat clients
        List,       //Get a list of users in the chat room from the server
        Null,
        MsgLen,        //No command
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

/*/
 * credit to:A Chat Application Using Asynchronous TCP Sockets by Hitesh Sharma for orignal implementation
 * code has now been reworked into a more modular format for general use.
/*/


namespace TFMS_Space
{
    public delegate void MessageRecieved(Data dataReceived);
    public struct ClientInfo
    {
        public Socket socket;
        public string strName;

        public ClientInfo(Socket a, string b)
        {
            socket = a;
            strName = b;
        }
    }
    public class TFMSServer
    {
        // info about connected clients
        public  const int BUFF_SIZE = 1024;
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
            byteData = new byte[BUFF_SIZE];
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
            int buffSize = BUFF_SIZE;
            Socket clientSocket = (Socket)ar.AsyncState;
            Console.WriteLine("End Receive from:{0}", getNamefromSocket(clientSocket));
            clientSocket.EndReceive(ar);
            Data msgReceived = new Data(byteData);

            Data msgToSend = new Data();


            msgToSend.cmdCommand = msgReceived.cmdCommand;
            msgToSend.strName = msgReceived.strName;

            switch (msgReceived.cmdCommand)
            {
                //When a user logs in to the server then we add her to our list of clients
                case Command.Login:
                    logonRequested(msgReceived);
                    ClientInfo clientInfo = new ClientInfo(clientSocket, msgReceived.strName);
                    clientList.Add(clientInfo);
                    Console.WriteLine("received login from:{0}", getNamefromSocket(clientSocket));
                    //just pass the name to other clients for now
                    // let the client program handle how to deal with it.
                    msgToSend.strMessage = msgReceived.strName;
                    break;
                case Command.Logout:
                    Console.WriteLine("Received Logout from:{0}", getNamefromSocket(clientSocket));
                    logoffRequested(msgReceived);
                    //find the index of the client whose socket we are provided
                    clientList.RemoveAt(findIndexFromClient(clientList, clientSocket));
                    //clientSocket.EndReceive(ar);
                    //Console.WriteLine("EndReceive:");
                    clientSocket.Close();
                    msgToSend.strMessage = msgReceived.strName;
                    break;
                case Command.Message:
                    Console.WriteLine("Received Message from:{0} size=({1})", getNamefromSocket(clientSocket),byteData.Length);
                    RelayRequested(msgReceived);
                    msgToSend.strMessage = msgReceived.strMessage;
                    break;
                case Command.List:
                    Console.WriteLine("Receved List Rqfrom:{0}", getNamefromSocket(clientSocket));
                    ListRequested(msgReceived);
                    msgToSend = GetClientListMessage();
                    //send to a single person
                    sendTFMSmsg(msgToSend, clientSocket, new AsyncCallback(OnSend));
                    break;
                case Command.MsgLen:

                    buffSize = int.Parse(msgReceived.strMessage);
                    Console.WriteLine("Received MsgLen:{0} from:{1}", buffSize, getNamefromSocket(clientSocket));
                    byteData = new byte[buffSize+BUFF_SIZE];
                    break;
                default:
                    Console.WriteLine("cant interpret command! aborting relay.");
                    Console.WriteLine("BeginRecive from :{0}", getNamefromSocket(clientSocket));
                    clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
                    return;
            }

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
            if (msgReceived.cmdCommand != Command.Logout)
            {
                Console.WriteLine("BeginRecive from :{0}", getNamefromSocket(clientSocket));
                clientSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnReceive), clientSocket);
            }

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
            Console.WriteLine("sending length: {0}",message.Length);
            s.Send(new Data(Command.MsgLen, string.Format("{0}",message.Length), d.strName).ToByte());
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
        public const int BUFF_SIZE = 1024;
        public int serverPort;
        public Socket clientSocket;
        public string strName;
        byte[] byteData;

        #region event delegates
        public event MessageRecieved loginReceived;
        public event MessageRecieved logoffReceived;
        public event MessageRecieved dataReceived;
        public event MessageRecieved listReceived;
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
            byteData = new byte[BUFF_SIZE];
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
                clientSocket.Send(msgToSend.ToByte());

                byteData = new byte[BUFF_SIZE];
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
            //Data lenToSend = new Data(Command.MsgLen, string.Format("{0}", data.Length), strName);
            //clientSocket.Send(lenToSend.ToByte());
            clientSocket.Send(data);
        }

        public void getList()
        {
            Data msgToSend = new Data(Command.List,null,strName);
            byte[] data = msgToSend.ToByte();
            Data lenToSend = new Data(Command.MsgLen, string.Format("{0}", data.Length), strName);
            clientSocket.Send(lenToSend.ToByte());
            clientSocket.BeginSend(data,0,data.Length,SocketFlags.None,new AsyncCallback(OnSend),null);
        }

        public void sendMessage(string data)
        {
            Data msgToSend = new Data(Command.Message, data, strName);
            byte[] d = msgToSend.ToByte();
            Data lenToSend = new Data(Command.MsgLen, string.Format("{0}", d.Length), strName);
            clientSocket.Send(lenToSend.ToByte());
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
            int bufSize = 1024;
            try
            {
                clientSocket.EndReceive(ar);

                Data msgReceived = new Data(byteData);
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
                        bufSize=int.Parse(msgReceived.strMessage);
                        
                        break;
                }

                byteData = new byte[bufSize+BUFF_SIZE];

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
            
        }
        #endregion
    }

    //The data structure by which the server and the client interact with 
    //each other
    public class Data
    {
        //Default constructor
        public Data()
            :this(Command.Null,null,null)
        { }

        public Data(Command c,string msg,string name)
        {
            cmdCommand = c; strMessage = msg; strName = name; acknowledged = false;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);
            if (Enum.GetName(typeof(Command), this.cmdCommand) == null)
            {
                Console.WriteLine("bad command...");
                return;
            }

            //The next four store the length of the name
            int nameLen = BitConverter.ToInt32(data, 4);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 8);

            //This check makes sure that strName has been passed in the array of bytes
            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 12, nameLen);
            else
                this.strName = null;

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage = Encoding.UTF8.GetString(data, 12 + nameLen, msgLen);
            else
                this.strMessage = null;
            acknowledged = false;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Length of the message
            if (strMessage != null)
                result.AddRange(BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            //And, lastly we add the message text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }

        public string strName;      //Name by which the client logs into the room
        public string strMessage;   //Message text
        public Command cmdCommand;  //Command type (login, logout, send message, etcetera)
        public bool acknowledged;
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

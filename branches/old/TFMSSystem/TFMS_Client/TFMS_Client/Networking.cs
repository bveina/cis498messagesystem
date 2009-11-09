using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace TFMS_Client
{
    public class NetworkMgr
    {
        public static int PORT = 2012;
        public Queue<string> cmd_queue = new Queue<string>();
        System.Net.EndPoint remote_addr;
        Socket socket;

        Thread listen_thread;

        private void ReceiveMessages()
        {
            byte[] data = new byte[1024];

            try
            {
                while (socket.Connected)
                {
                    data = new byte[1024];
                    int recv = socket.ReceiveFrom(data, ref remote_addr);
                    string msg = System.Text.Encoding.ASCII.GetString(data, 0, recv);
                    cmd_queue.Enqueue(msg);
                }
            }
            catch (Exception)
            { }
        }

        public bool doConnect(string server, string user)
        {
            //Try to open the socket
            bool connected = socketConnect(server);

            //Launch the main UI if connected
            if (connected)
            {
                remote_addr = socket.RemoteEndPoint;
                System.Threading.Thread.Sleep(800);

                cmdSend("name*" + user);

                //Start receiving messages
                ThreadStart job = new ThreadStart(ReceiveMessages);
                listen_thread = new Thread(job);
                listen_thread.Start();
                return true;
            }
            else
            {
                System.Threading.Thread.Sleep(800);
                stopConnect();
                return false;
            }
        }

        public bool socketConnect(string server)
        {
            bool result = false;
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(server, PORT);
                    result = true;
                }
                catch (Exception)
                { }
            }
            System.Threading.Thread.Sleep(800);
            return result;
        }

        public void stopConnect()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
                remote_addr = null;

                if (listen_thread != null)
                    listen_thread.Abort();
            }
        }

        public void sendMessage(string msg)
        {
            cmdSend("msg*" + msg);
        }

        public void cmdSend(string s)
        {
            if (socket != null && socket.Connected)
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
                socket.Send(data);
            }
        }

        public bool isConnected()
        {
            return socket != null && socket.Connected;
        }
    }
}

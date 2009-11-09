using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace TFMS_Server
{
    class ClientConnection
    {
        public static int RATE = 1;

        public enum ConnectionState {Active, Dead, Pending};
        public ConnectionState connection_state;

        static Queue<string> cmd_queue = new Queue<string>();
        private Queue<string> pending_messages = new Queue<string>();
        System.Windows.Forms.Timer timer;
        Socket client;
        System.Net.EndPoint remote_addr;

        string name = "New Client";
        public float timeout;

        Thread listen_thread;
        DateTime lastTime = DateTime.Now;

        public ClientConnection()
        {
            connection_state = ConnectionState.Pending;

            //Start the timer and set the tick function
            Functions.startTimer(ref timer, RATE);
            timer.Tick += new EventHandler(timer_Tick);

            resetTimeout();
        }

        /// <summary>
        /// Timer ticks for each client connection cycle.
        /// </summary>
        public void timer_Tick(object sender, EventArgs e)
        {
            switch (connection_state)
            {
                case ConnectionState.Active:
                    //Check if connection dead
                    /*timeout -= 1;
                    if (timeout <= 0)
                    {
                        Disconnect();
                        return;
                    }*/

                    TimeSpan ts = DateTime.Now.Subtract(lastTime);
                    if (ts.Seconds > 10)
                    {
                        lastTime = DateTime.Now;
                        cmdSend("ping");
                    }

                    //Handle messages
                    handleCmds();
                    break;
                case ConnectionState.Pending:
                    //Check the listener
                    break;
            }
        }

        public void cmdSend(string s)
        {
            if (client != null && client.Connected)
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(s);
                client.Send(data);
            }
        }

        public void createConnection(ref Socket cli)
        {
            this.client = cli;
            connection_state = ConnectionState.Active;
            remote_addr = client.RemoteEndPoint;
            name = remote_addr.ToString();

            //Start receiving messages
            ThreadStart job = new ThreadStart(ReceiveMessages);
            listen_thread = new Thread(job);
            listen_thread.Start();
        }

        private void ReceiveMessages()
        {
            byte[] data = new byte[1024];

            try
            {
                while (client.Connected)
                {
                    data = new byte[1024];
                    int recv = client.ReceiveFrom(data, ref remote_addr);
                    string msg = System.Text.Encoding.ASCII.GetString(data, 0, recv);
                    cmd_queue.Enqueue(msg);
                }
            }
            catch (Exception)
            { }
        }

        private void handleCmds()
        {
            while (cmd_queue.Count > 0)
            {
                string[] cmds = cmd_queue.Dequeue().Split('*');

                switch (cmds[0])
                {
                    case "quit":
                        Disconnect();
                        break;
                    case "name":
                        name = cmds[1];
                        break;
                    case "msg":
                        pending_messages.Enqueue(cmds[1]);
                        break;
                    case "ping":
                        resetTimeout();
                        break;
                }
            }
        }

        private void resetTimeout()
        {
            timeout = 3000;
        }

        public string getName()
        {
            return this.name;
        }

        /// <summary>
        /// Called in order to disconnect this client. Will be marked as a dead client
        /// and removed from the client list during the next main cycle.
        /// </summary>
        public void Disconnect()
        {
            //Abort listening thread
            listen_thread.Abort();
            //Dequeue all commands
            while (cmd_queue.Count > 0)
                cmd_queue.Dequeue();
            //Set connection state as dead
            this.connection_state = ConnectionState.Dead;
        }

        public string getIP()
        {
            return client.RemoteEndPoint.ToString();
        }

        public bool hasPendingMessages()
        {
            if (pending_messages.Count > 0)
                return true;
            return false;
        }

        public string getPendingMessage()
        {
            return pending_messages.Dequeue();
        }
    }
}
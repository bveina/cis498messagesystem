using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Windows.Forms;

namespace TFMS_Server
{
    public partial class Form1 : Form
    {
        public static int PORT = 2012;
        public static string LOCAL_ADDR = "127.0.0.1";

        List<ClientConnection> clients;

        Timer timer;
        TcpListener listener;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            startServer();
        }

        /// <summary>
        /// Timer ticks for each server cycle.
        /// </summary>
        void timer_Tick(object sender, EventArgs e)
        {
            //Check status of connections
            for (int i = 0; i < clients.Count; i++)
            {
                //If client is dead, remove it from the list
                if (clients[i].connection_state == ClientConnection.ConnectionState.Dead)
                {
                    clients[i].Disconnect();
                    clients.RemoveAt(i);
                    i--;
                }
            }

            //Check for new connections
            while (listener.Pending())
            {
                Socket client = listener.AcceptSocket();
                clients[clients.Count - 1].createConnection(ref client);
                newPendingClient();
            }

            //Accept/Relay new messages
            relayMessages();
            
            //Update the client list
            lstClients.Items.Clear();
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].connection_state == ClientConnection.ConnectionState.Active)
                {
                    string txt = i + ": " + clients[i].getName() + " [" + clients[i].getIP() + "]";
                    txt += " <" + clients[i].timeout;
                    lstClients.Items.Add(txt);
                }
                else if (clients[i].connection_state == ClientConnection.ConnectionState.Pending)
                    lstClients.Items.Add("Pending Client");
                else
                    lstClients.Items.Add("Dead Client");
            }
        }

        //Called to add a new pending connection to the end of the client list
        private void newPendingClient()
        {
            ClientConnection newclient = newClientConnection();
            clients.Add(newclient);
        }

        private void startServer()
        {
            statusLabel.Text = "Starting server...";

            System.Net.IPAddress localaddr = System.Net.IPAddress.Parse(LOCAL_ADDR);
            listener = new TcpListener(localaddr, PORT);
            listener.Start();

            //Do initialization
            clients = new List<ClientConnection>();
            clients.Add(newClientConnection());    //Add a pending client

            //Start timer and set tick function
            Functions.startTimer(ref timer, 1);
            timer.Tick += new EventHandler(timer_Tick);

            statusLabel.Text = "Server running.";
        }

        private void closeServer()
        {
            statusLabel.Text = "Server closed.";
        }

        private ClientConnection newClientConnection()
        {
            ClientConnection cl = new ClientConnection();
            return cl;
        }

        private void relayMessages()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                while (clients[i].hasPendingMessages())
                {
                    statusLabel.Text = "Relaying message from client " + clients[i].getName();
                    string relay_msg = "msg*" + clients[i].getName() + "*" + clients[i].getPendingMessage();
                    for (int j = 0; j < clients.Count; j++)
                    {
                        if (clients[j].connection_state == ClientConnection.ConnectionState.Active)
                        {
                            //If I != J
                            clients[j].cmdSend(relay_msg);
                        }
                    }
                }
            }
        }
    }
}

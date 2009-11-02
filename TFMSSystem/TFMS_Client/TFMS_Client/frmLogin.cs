using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace TFMS_Client
{
    public partial class frmLogin : Form
    {
        public static string SERVER = "127.0.0.1";
        public static int RATE = 2;

        public static NetworkMgr network_mgr = new NetworkMgr();
        frmMain main;

        Timer timer;

        DateTime lastDate;

        public frmLogin()
        {
            InitializeComponent();
            main = new frmMain();
            main.VisibleChanged += new EventHandler(main_VisibleChanged);
            main.FormClosed += new FormClosedEventHandler(main_FormClosed);
        }

        void main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (main.close_state == 0)
                System.Environment.Exit(0);
        }

        /// <summary>
        /// Triggered by the main form being hidden. Depending on why it closed do different things.
        /// </summary>
        void main_VisibleChanged(object sender, EventArgs e)
        {
            if (!main.Visible)
            {
                if (main.close_state < 2)
                {
                    //Tell the server you are quitting
                    if (network_mgr.isConnected())
                    {
                        network_mgr.cmdSend("quit");
                        network_mgr.stopConnect();
                        setView(0);
                    }
                }

                //User was returning to login
                if (main.close_state == 1)
                {
                    displayLogin();
                }
                else //User closing the program
                    System.Environment.Exit(0);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            //Change UI to show status of login process
            setView(1);
            setStatus("Connecting to server...");

            if (network_mgr.doConnect(txtServer.Text, txtUser.Text))
            {
                setStatus("Connected to server.");
                beginTimer();
                displayMain();
            }
            else
            {
                setStatus("Failed to connect.");
                displayLogin();
            }
        }

        private void setStatus(string s)
        {
            lblStatus.Text = s;
            lblStatus.Refresh();
        }

        private void beginTimer()
        {
            timer = new Timer();
            timer.Interval = RATE;
            timer.Start();
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //This method will receive all new messages
            if (network_mgr.isConnected())
            {
                while (network_mgr.cmd_queue.Count > 0)
                {
                    string[] cmds = network_mgr.cmd_queue.Dequeue().Split('*');

                    switch (cmds[0])
                    {
                        case "msg":
                            if (main != null && main.Visible)
                                main.addMessage(cmds[2], cmds[1]);
                            break;
                    }
                }
            }
        }

        private void stopTimer()
        {
            timer.Stop();
            timer.Dispose();
        }

        //Display the main UI 
        private void displayMain()
        {
            main.Show();
            this.Hide();
        }

        //Display the login UI
        private void displayLogin()
        {
            main.Hide();
            this.Show();
            setView(0);
        }

        private void setView(int i)
        {
            switch (i)
            {
                case 0:
                    //Show username input
                    groupBoxInput.Location = new Point(13, 169);
                    groupBox1.Location = new Point(999, 999);
                    break;
                case 1:
                    //Show login process
                    groupBoxInput.Location = new Point(999, 999);
                    groupBox1.Location = new Point(13, 169);
                    break;
            }
            this.Refresh();
        }

        private void btnCancelConnect_Click(object sender, EventArgs e)
        {
            network_mgr.stopConnect();
            setView(0);
        }

        private void lblStatus_Click(object sender, EventArgs e)
        {

        }
    }
}

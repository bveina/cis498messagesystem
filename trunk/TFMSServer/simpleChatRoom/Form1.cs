using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TFMS_Space;

namespace simpleChatRoom
{
    
    public partial class Form1 : Form
    {
        public TFMSClient myClient;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            splitContainer1.Panel2.Enabled = false;
            cmdLogout.Enabled = false;
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, null);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            myClient = new TFMSClient(int.Parse(txtPort.Text), txtName.Text);
            myClient.dataReceived += new MessageRecieved(handleMessage);
            myClient.listReceived += new MessageRecieved(handleList);
            myClient.loginReceived += new MessageRecieved(handleLogin);
            myClient.logoffReceived += new MessageRecieved(handleLogoff);
            if (myClient.connect("127.0.0.1"))
            {
                MessageBox.Show("connected");
                splitContainer1.Panel2.Enabled = true;
                txtName.Enabled = false;
                txtPort.Enabled = false;
                cmdLogout.Enabled = true;
                myClient.getList();
            }
            else
            {
                MessageBox.Show("connection failed");
            }
        }

        void handleLogin(Data msg)
        {
            if (msg.strName == txtName.Text) return;
            if (lstClients.Items.Contains(msg.strName)) return;
            else lstClients.Items.Add(msg.strName);
            txtChat.AppendText(string.Format("{0} has joined the chat\r\n",msg.strName));
            myClient.getList();
        }
        void handleLogoff(Data msg)
        {
            lstClients.Items.Remove(msg.strName);
            txtChat.AppendText(string.Format("{0} has left the chat\r\n", msg.strName));
            myClient.getList();
        }

        void handleList(Data msg)
        {
            string[] clients = msg.strMessage.Split('*');
            lstClients.SuspendLayout();
            lstClients.Items.Clear();
            lstClients.Items.AddRange(clients);
            lstClients.ResumeLayout();
        }
        void handleMessage(Data msg)
        {
            txtChat.AppendText(string.Format("{0}: {1}\r\n",msg.strName,msg.strMessage));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtMsg.Text.Length == 0) return;
            myClient.sendMessage(txtMsg.Text);
        }

        private void cmdLogout_Click(object sender, EventArgs e)
        {
            myClient.disconnect();
            button1.Enabled = true;
            splitContainer1.Panel2.Enabled = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            myClient.disconnect();
        }


    }
}

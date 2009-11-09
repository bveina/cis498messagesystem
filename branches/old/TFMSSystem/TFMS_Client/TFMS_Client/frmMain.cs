using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TFMS_Client
{
    public partial class frmMain : Form
    {
        public int close_state = 0;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        void frmMain_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            close_state = 0;
            e.Cancel = true;
            this.Hide();
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            close_state = 1;
            this.Hide();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                frmLogin.network_mgr.sendMessage(textBox1.Text);
            }
        }

        public void addMessage(string msg, string sender)
        {
            listBox1.Items.Add("(" + sender + ") " + msg);
        }
    }
}

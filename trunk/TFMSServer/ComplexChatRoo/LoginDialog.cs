using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ComplexChatRoo
{
    public partial class LoginDialog : Form
    {
        public string name;
        public string serverAddr;
        public string serverPort;
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            name = txtUser.Text;
            serverAddr = txtServer.Text;
            serverPort = txtPort.Text;
            DialogResult = DialogResult.OK;
        }
    }
}

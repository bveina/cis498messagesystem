using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ComplexChatRoom
{
    /// <summary>
    /// The login form that the user encounters when they begin the TFMS program
    /// </summary>
    public partial class LoginDialog : Form
    {
        #region LoginDialog class variables

        /// <summary>
        /// The user name that the user would like to appear as on the system
        /// </summary>
        public string name;

        /// <summary>
        /// The server address to attempt to contact
        /// </summary>
        public string serverAddr;

        #endregion

        #region LoginDialog constructors

        /// <summary>
        /// Simply initializes the component, the Form will take over from here
        /// </summary>
        public LoginDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region LoginDialog event actions

        private void button1_Click(object sender, EventArgs e)
        {
            name = txtUser.Text;
            serverAddr = txtServer.Text;
            DialogResult = DialogResult.OK;
        }

        #endregion
    }
}

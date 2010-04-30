using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TFMS_Space;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ComplexChatRoom
{
    /// <summary>
    /// The TFMS_GUI class provides a simple way for the user to log on in to the Tactical Field Message System
    /// TFMS_GUI starts by prompting the user for a user name, IP address, and destination port number
    /// If the credentials check out, the user is provided an interface with sending and receiving capabilities
    /// In the "Compose" tab the user can create and send messages, while in the "Acknowledge" tab the user can view messages
    /// </summary>
    public partial class TFMS_GUI : Form
    {
        #region TFMS_GUI class variables

        /// <summary>
        /// The user's login name
        /// </summary>
        string name;

        /// <summary>
        /// The IP address of the dedicated TFMS server
        /// </summary>
        string serverAddress;

        /// <summary>
        /// The port number of the TFMS server at the IP address
        /// </summary>
        int serverPort;

        /// <summary>
        /// The TFMS_Client object, which represents the user's session in the system
        /// </summary>
        TFMS_Client myClient;

        #endregion

        #region TFMS_GUI constructors

        public TFMS_GUI()
        {
            InitializeComponent();
        }

        #endregion

        #region TFMS_GUI event actions

        private void Form1_Load(object sender, EventArgs e)
        {
            LoginDialog login = new LoginDialog();
            DialogResult r;
            do
            {
                r = login.ShowDialog();
                if (r == DialogResult.OK)
                {
                    // get the user's name, IP address, and port number from the LoginDialog
                    name = login.name;
                    serverAddress = login.serverAddr;
                    serverPort = int.Parse(login.serverPort);

                    // create a new TFMS_Client object 
                    myClient = new TFMS_Client(serverPort, name);

                    // create the TFMS_Client delegates
                    myClient.loginReceived += new TFMS_MessageRecieved(handleLogon);
                    myClient.logoffReceived += new TFMS_MessageRecieved(handleLogoff);
                    myClient.listReceived += new TFMS_MessageRecieved(handleList);
                    myClient.dataReceived += new TFMS_MessageRecieved(handleMessage);
                    myClient.disconnectDetected += new TFMS_MessageRecieved(myClient_disconnectDetected);
                }
                else if (r == DialogResult.Cancel)
                {
                    this.Close();
                    return;
                }

            } while (myClient == null || !myClient.connect(serverAddress));

            // set the form title to show the user's login name
            this.Text = name; 

            this.Visible = true;
            this.WindowState = FormWindowState.Maximized;


            #region THIS IS A HACK SO WE DON'T POST THE IMAGE ONLINE
            try
            {
                drawingBox31.BackgroundImage = Image.FromFile("tfmsimage.jpg");
                vectorBox1.BackgroundImage = Image.FromFile("tfmsimage.jpg");
            }
            catch (Exception)
            {
                Console.WriteLine("Image file not found!");
            }
            #endregion
        }

        #endregion

        #region TFMS_GUI helper methods

        private void myClient_disconnectDetected(TFMS_Data dataReceived)
        {
            //this.Close();
            myClient = null;
            MessageBox.Show("The server has been closed or has crashed. Please restart the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }


        private void handleLogon(TFMS_Data msg)
        {
            notifyIcon1.BalloonTipText = string.Format("{0} has joined", msg.strName);
            notifyIcon1.ShowBalloonTip(500);
        }
        
        private void handleLogoff(TFMS_Data msg)
        {
            notifyIcon1.BalloonTipText = string.Format("{0} has left", msg.strName);
            notifyIcon1.ShowBalloonTip(500);
        }
        
        private void handleMessage(TFMS_Data msg)
        {

            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = string.Format("You have a TFM");
            notifyIcon1.ShowBalloonTip(5000);
            // make the call to add an item thread safe because chances are that this will be called from another thread
            if (lstMessages.InvokeRequired) 
                lstMessages.Invoke(new Action<TFMS_Data>(delegate(TFMS_Data a) { lstMessages.Items.Add(a); }), msg);
            else
                lstMessages.Items.Add(msg);
        }
        
        private void handleList(TFMS_Data msg)
        {
            notifyIcon1.ShowBalloonTip(500, "List", "you got the list of peers", ToolTipIcon.Info);
        }

        #endregion

        #region TFMS_GUI event actions
        
        /// <summary>this sends the current image to the server.</summary>
        private void cmdSend_Click(object sender, EventArgs e)
        {
            String message = drawingBox31.serialize();
            if (drawingBox31.lines.Count == 0)
                MessageBox.Show("You haven't written anything!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                myClient.sendMessage(message);
                drawingBox31.Clear();
            }
        }
        
        /// <summary>if we are exiting we should disconnect from the server.</summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            if (myClient != null)
                myClient.disconnect();
        }

        /// <summary>
        /// When a user clicks on a message in the "Acknowledged" tab, it should be displayed, flagged as "read", and change colors
        /// </summary>
        /// <param name="sender">mandatory parameter</param>
        /// <param name="e">current event action</param>
        private void lstMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstMessages.SelectedItem == null) return;

            try
            {
                // retrieve the TFMS_Data for the currently selected message
                List<DrawingBox.PathData> myPaths;
                TFMS_Data myData = (TFMS_Data)lstMessages.SelectedItem;

                // set the acknowledged flag
                myData.acknowledged = true;

                string mystr = myData.strMessage;
                string[] items = mystr.Split(',');
                string hash = items[2];

                // convert the message from a string to a displayable image
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(hash)))
                {
                    BinaryFormatter xs = new BinaryFormatter();
                    myPaths = (List<DrawingBox.PathData>)xs.Deserialize(ms);
                    ms.Close();
                }
                

                vectorBox1.SuspendLayout();
                vectorBox1.pathWidth = int.Parse(items[0]);
                vectorBox1.pathHeight = int.Parse(items[1]);
                vectorBox1.Paths = myPaths;
                vectorBox1.ResumeLayout();
                vectorBox1.Invalidate();
                lstMessages.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "There was an error displaying the image!");
            }
        }

        /// <summary>
        /// Necessary method that is being overwritten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vectorBox1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the custom drawing of the individual items on the list box of messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstMessages_DrawItem(object sender, DrawItemEventArgs e)
        {
            TFMS_Data tmp = (TFMS_Data)lstMessages.Items[e.Index];
            string dispString = string.Format("{0} - {1}", tmp.strName, tmp.timeStamp);

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                // Draw the background of the ListBox control for each item.
                e.DrawBackground();
                e.Graphics.DrawString(dispString,
                    e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
            }
            else
            {
                // Define the default color of the brush as black.
                Brush myBrush = Brushes.Black;

                // Determine the color of the brush to draw each item based 
                // on the index of the item to draw.
                myBrush = ((TFMS_Data)lstMessages.Items[e.Index]).dispColor;

                e.Graphics.FillRectangle(myBrush, e.Bounds);
                // Draw the current item text based on the current Font 
                // and the custom brush settings.
                
                e.Graphics.DrawString(dispString,
                    e.Font, Brushes.Black, e.Bounds, StringFormat.GenericDefault);

            }
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            NotifyIcon ni = (NotifyIcon)sender;
            this.BringToFront();
        }

        private void cmdCopy_Click(object sender, EventArgs e)
        {
            drawingBox31.lines = vectorBox1.Paths;
        }

        #endregion
    }
}

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
    public partial class TFMS_GUI : Form
    {
        string name;
        string serverAddress;
        int serverPort;
        TFMS_Client myClient;
        public TFMS_GUI()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            #region get server and client info and try to login to the server
            LoginDialog login = new LoginDialog();
            DialogResult r;
            do
            {
                r = login.ShowDialog();
                if (r == DialogResult.OK)
                {
                    name = login.name;
                    serverAddress = login.serverAddr;
                    serverPort = int.Parse(login.serverPort);
                    myClient = new TFMS_Client(serverPort, name);
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
            #endregion
            this.Text = name; // set the form title to show your name

            this.Visible = true;
            this.WindowState = FormWindowState.Maximized;

            #region THIS IS A HACK SO WE DON'T POST THE IMAGE ONLINE
            drawingBox31.BackgroundImage = Image.FromFile("tfmsimage.jpg");
            vectorBox1.BackgroundImage = Image.FromFile("tfmsimage.jpg");
            #endregion
        }

        #region message handling routines
        void myClient_disconnectDetected(TFMS_Data dataReceived)
        {
            //this.Close();
            myClient = null;
            MessageBox.Show("The server has probably blown up. We will now exit", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        void handleLogon(TFMS_Data msg)
        {
            notifyIcon1.BalloonTipText = string.Format("{0} has joined", msg.strName);
            notifyIcon1.ShowBalloonTip(500);
        }
        void handleLogoff(TFMS_Data msg)
        {
            notifyIcon1.BalloonTipText = string.Format("{0} has left", msg.strName);
            notifyIcon1.ShowBalloonTip(500);
        }
        void handleMessage(TFMS_Data msg)
        {

            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = string.Format("you've got TFMS");
            notifyIcon1.ShowBalloonTip(5000);
            // make the call to add an item thread safe because chances are that this will be called from another thread
            if (lstMessages.InvokeRequired) 
                lstMessages.Invoke(new Action<TFMS_Data>(delegate(TFMS_Data a) { lstMessages.Items.Add(a); }), msg);
            else
                lstMessages.Items.Add(msg);
        }
        void handleList(TFMS_Data msg)
        {
            notifyIcon1.ShowBalloonTip(500, "List", "you got the list of peers", ToolTipIcon.Info);
        }
        #endregion

        /// <summary>this sends the current image to the server.</summary>
        private void cmdSend_Click(object sender, EventArgs e)
        {
            String message = drawingBox31.serialize();
            if (drawingBox31.lines.Count == 0)
                MessageBox.Show("You haven't written anything!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        /// <summary> when the user clicks on a message it should be displayed in the vector box. </summary>
        private void lstMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region use the path data to recreate the image on the vectorbox

            if (lstMessages.SelectedItem == null) return;
            try
            {
                List<DrawingBox.PathData> myPaths;
                TFMS_Data myData = (TFMS_Data)lstMessages.SelectedItem;
                myData.acknowledged = true;
                string mystr = myData.strMessage;
                string[] items = mystr.Split(',');
                string hash = items[2];


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
                MessageBox.Show(ex.Message, "there was an error displaying the image");
            }
            #endregion
        }

        private void drawingBox31_Load(object sender, EventArgs e)
        {

        }

        private void vectorBox1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>this handles the custom drawing of the individual items on the list box of messages</summary>
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
    }
}

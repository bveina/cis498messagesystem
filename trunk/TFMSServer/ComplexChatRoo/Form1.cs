using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TFMS_Space;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml;
using System.Runtime.Serialization.Formatters.Binary;

namespace ComplexChatRoo
{
    public partial class Form1 : Form
    {
        string name;
        string serverAddress;
        int    serverPort;
        TFMSClient myClient;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
                    myClient = new TFMSClient(serverPort, name);
                    myClient.loginReceived += new MessageRecieved(handleLogon);
                    myClient.logoffReceived += new MessageRecieved(handleLogoff);
                    myClient.listReceived += new MessageRecieved(handleList);
                    myClient.dataReceived += new MessageRecieved(handleMessage);
                }
                else if (r == DialogResult.Cancel)
                {
                    this.Close();
                    return;
                }
                
            } while (myClient==null || !myClient.connect(serverAddress));
            myClient.getList();
        }


        void handleLogon(Data msg)
        {
            notifyIcon1.BalloonTipText = string.Format("{0} has joinHed", msg.strName);
            notifyIcon1.ShowBalloonTip(500);
        }
        void handleLogoff(Data msg)
        {
            notifyIcon1.BalloonTipText = string.Format("{0} has left", msg.strName);
            notifyIcon1.ShowBalloonTip(500);
        }
        void handleMessage(Data msg)
        {
            
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipText = string.Format("you've got TFMS");
            notifyIcon1.ShowBalloonTip(5000);
            if (lstMessages.InvokeRequired)
            {
                lstMessages.Invoke(new Action<string>(delegate(string a){lstMessages.Items.Add(a);}),msg.strMessage);
            }
            else
            {
                lstMessages.Items.Add(msg.strMessage);
            }
        }
        void handleList(Data msg)
        {
            notifyIcon1.ShowBalloonTip(500, "List", "you got the list of peers", ToolTipIcon.Info);
        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
            myClient.sendMessage(drawingBox31.serialize());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myClient!=null)
                myClient.disconnect();
        }

        private void lstMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region (an attempt to use the path data)
            if (lstMessages.SelectedItem == null) return;
            List<DrawingBox.PathData> myPaths;
            string mystr = (string)lstMessages.SelectedItem;
            string[] items = mystr.Split(',');
            string hash = items[2];
            BinaryFormatter  xs = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(Convert.FromBase64String( hash));
            myPaths = (List<DrawingBox.PathData>)xs.Deserialize(ms);
            ms.Close();

            int w = pictureBox1.ClientRectangle.Width;
            int h = pictureBox1.ClientRectangle.Height;
            Bitmap temp = new Bitmap(w,h);
            Graphics g = Graphics.FromImage(temp);
            PointF[] newRect={new PointF(0,0),new PointF(w,0),
                              new PointF(0,h),new PointF(w,h)};
            Rectangle srcRect= new Rectangle(0,0,int.Parse(items[0]),int.Parse(items[1]));
            GraphicsPath gp;
            foreach (DrawingBox.PathData pd in myPaths)
            {
                if (pd.path == null) continue;
                Pen p = new Pen(pd.pathColor, pd.pathWidth);
                pd.path.Warp(newRect, srcRect);
                g.DrawPath(p,pd.path);
                p.Dispose();
            }
            pictureBox1.Image  = temp;
            
            #endregion
            /*
             * 
             * string msg = (string)lstMessages.SelectedItem;
            string[] objs = msg.Split('*');
            string hash = objs[2];
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(hash));
            Bitmap temp = new Bitmap(ms);
            
            pictureBox1.Image = temp;
             */
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace DrawingBox
{
    /// <summary>
    /// ClientDrawingBox is the control used to allow the user to easily draw a Tactical Field Message
    /// The user can choose pen or eraser, line width, line color, and undo
    /// </summary>
    public partial class ClientDrawingBox : UserControl
    {
        #region ClientDrawingBox class variables

        /// <summary>
        /// Default width of a large brush
        /// </summary>
        private int BigSize = 10;

        /// <summary>
        /// Default width of a medium brush
        /// </summary>
        private int MedSize = 4;
        
        /// <summary>
        /// Default width of a small brush
        /// </summary>
        private int SmlSize = 1;

        /// <summary>
        /// The current path that is being constructed by the user (mousebutton is down and a user is currently drawing)
        /// </summary>
        private PathData currentPath;
        
        /// <summary>
        /// All lines are treated as vectors for easy scaling
        /// </summary>
        private List<PathData> myPaths;
        
        /// <summary>
        /// Helps dictate to the client when it is allowed to draw
        /// </summary>
        private bool shouldPaint;
        
        /// <summary>
        /// The last point the mouse was recorded as being at (updated everytime a mouse move event is raised)
        /// </summary>
        private Point lastLocation;

        /// <summary>
        /// The currently selected color
        /// </summary>
        private Color myColor;
        
        /// <summary>
        /// The currently selected width
        /// </summary>
        private int myWidth;
        
        /// <summary>
        /// The currently selected drawing mode, which is either erasing or not (erasing or drawing)
        /// </summary>
        private bool eraseMode;

        #endregion

        #region ClientDrawingBox constructors

        /// <summary>
        /// boilerplate code constructor
        /// </summary>
        public ClientDrawingBox()
        {
            InitializeComponent();
        }

        #endregion

        #region ClientDrawingBox "get" and "set" overridden methods

        /// <summary>
        /// the color that is currently selected
        /// </summary>
        public Color lineColor
        {
            get { return myColor; }
            set { myColor = value; }
        }
      
        /// <summary>
        /// the line width that is currently selected
        /// </summary>
        public int lineWidth
        {
            get { return myWidth; }
            set { myWidth = value; }
        }
        
        /// <summary>
        /// the list of all lines in the drawing box
        /// </summary>
        public List<PathData> lines
        {
            get { return myPaths; }
            set { myPaths = value; }
        }

        #endregion

        #region ClientDrawingBox helper methods

        /// <summary>
        /// Clears the entire drawing surface
        /// </summary>
        public void Clear()
        {
            myPaths.Clear();
            currentPath = new PathData();
            this.Invalidate();
        }

        /// <summary>
        /// Flattens the vectors into a single bitmap
        /// </summary>
        /// <returns>a bitmap image</returns>
        public Bitmap getImage()
        {
            Bitmap temp = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(temp);
            foreach (PathData pd in myPaths)
                g.DrawPath(new Pen(pd.pathColor, pd.pathWidth), pd.path);
            return temp;
        }

        /// <summary>
        /// Converts the list of PathData from an image into a string
        /// </summary>
        /// <returns>a string representation of the image</returns>
        public string serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter xser = new BinaryFormatter();
                xser.Serialize(ms, myPaths);
                ms.Seek(0, SeekOrigin.Begin);
                string result = Convert.ToBase64String(ms.GetBuffer());
                return string.Format("{0},{1},{2}", this.ClientRectangle.Width, this.ClientRectangle.Height - this.toolStrip1.Height, result);
            }
        }

        /// <summary>
        /// Identifies whether the given point is located on the specified PathData object
        /// </summary>
        /// <param name="path">the current path</param>
        /// <param name="p">the point being sought</param>
        /// <returns>true if the PathData contains the point, otherwise false</returns>
        public bool isPointOnPath(PathData path, Point p)
        {
            PointF a, b;
            Rectangle c;
            if (path.path.PointCount == 0) return false;
            for (int i = 0; i < path.path.PathPoints.Length - 1; i++)
            {
                a = path.path.PathPoints[i];
                b = path.path.PathPoints[i + 1];
                c = new Rectangle(
                     (int)Math.Min(a.X, b.X),
                     (int)Math.Min(a.Y, b.Y),
                     (int)Math.Abs(a.X - b.X) + path.pathWidth + 1,
                     (int)Math.Abs(a.Y - b.Y) + path.pathWidth + 1);
                if (c.Contains(p)) return true;
            }

            return false;
        }
        
        /// <summary>
        /// Get the index of the point in the list of PathData objects
        /// </summary>
        /// <param name="myPaths">the list of paths</param>
        /// <param name="p">the point being sought</param>
        /// <returns>the index of the path containing the point, otherwise -1</returns>
        public int getPathIndexFromPoint(List<PathData> myPaths, Point p)
        {
            foreach (PathData pd in myPaths)
            {
                if (isPointOnPath(pd, p)) return myPaths.IndexOf(pd);
            }
            return -1;
        }


        #endregion

        #region ClientDrawingBox event actions

        private void DrawingBox3_Load(object sender, EventArgs e)
        {
            currentPath = new PathData(new GraphicsPath(), myColor, myWidth);
            myPaths = new List<PathData>();
            eraseMode = false;
            cmdDraw.Checked = true;
            cmdSizeTiny.Checked = true;
            this.myColor = Color.Black;
            this.myWidth = 1;
            
        }
        private void DrawingBox_Resize(object sender, EventArgs e)
        {
            ClientDrawingBox mySender = (ClientDrawingBox)sender;
            mySender.Invalidate();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            cmdErase.Checked = false;
            eraseMode = false;
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            cmdDraw.Checked = false;
            eraseMode = true;
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            cmdSizeMid.Checked = false;
            cmdSizeTiny.Checked = false;
            this.myWidth = this.BigSize;
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            cmdSizeBig.Checked = false;
            cmdSizeTiny.Checked = false;
            this.myWidth = this.MedSize;
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            cmdSizeBig.Checked = false;
            cmdSizeMid.Checked = false;
            this.myWidth = this.SmlSize;
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (this.myPaths.Count < 1) return;
            this.myPaths.RemoveAt(this.myPaths.Count - 1);
            this.Invalidate();
        }

        private void DrawingBox3_MouseDown(object sender, MouseEventArgs e)
        {
            if (eraseMode == true) return;
            shouldPaint = true;
            currentPath = new PathData(new GraphicsPath(), myColor, myWidth);
            lastLocation = e.Location;
            Rectangle myBounds = this.ClientRectangle;
            myBounds.Height = this.ClientRectangle.Height - this.toolStrip1.Height;
            Cursor.Clip = this.RectangleToScreen(myBounds);
        }
        private void DrawingBox3_MouseMove(object sender, MouseEventArgs e)
        {
            if (eraseMode == true)
            {
                //if (isPointOnPath(e.Location))
                //    this.FindForm().Text = "OnPath";
                //else this.FindForm().Text="not onpath";
            }
            if (!shouldPaint) return;
            if (lastLocation == e.Location) return;
            currentPath.path.AddLine(lastLocation, e.Location);
            //Todo: invalidate less...
            this.Invalidate(new Rectangle(Math.Min(lastLocation.X, e.X) - myWidth / 2,
                                          Math.Min(lastLocation.Y, e.Y) - myWidth / 2,
                                          Math.Abs(lastLocation.X - e.X) + myWidth,
                                          Math.Abs(lastLocation.Y - e.Y) + myWidth));
            lastLocation = e.Location;
        }
        private void DrawingBox3_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Clip = Screen.PrimaryScreen.Bounds;
            int index;
            if (eraseMode)
            {
                if ((index = getPathIndexFromPoint(myPaths, e.Location))!=-1)
                    myPaths.RemoveAt(index);
                this.Invalidate();
            }
            else
            {

                shouldPaint = false;
                if (currentPath != null)
                {
                    myPaths.Add(currentPath);
                    currentPath = null;
                    this.Invalidate();
                }
            }
        }

        private void DrawingBox3_Paint(object sender, PaintEventArgs e)
        {
            Rectangle temp = this.ClientRectangle;
            temp.Height = temp.Height - this.toolStrip1.Height;
             e.Graphics.Clear(this.BackColor);
            if (this.BackgroundImage != null)
            {
                if (this.BackgroundImageLayout == ImageLayout.None)
                    e.Graphics.DrawImage(this.BackgroundImage, 0, 0);
                else if (this.BackgroundImageLayout == ImageLayout.Stretch)
                    e.Graphics.DrawImage(this.BackgroundImage, temp);
                else if (this.BackgroundImageLayout == ImageLayout.Center)
                    e.Graphics.DrawImageUnscaled(this.BackgroundImage, this.Width - this.BackgroundImage.Width / 2, this.Height - this.BackgroundImage.Height / 2);
            }

            if (myPaths != null)
            {
                foreach (PathData pd in myPaths)
                {
                    if (pd.path == null) continue;
                    Pen p = new Pen(pd.pathColor, pd.pathWidth);
                    e.Graphics.DrawPath(p, pd.path);
                    p.Dispose();
                }
            }
            if (currentPath !=null && currentPath.path != null)
            {
                Pen t = new Pen(currentPath.pathColor, currentPath.pathWidth);
                e.Graphics.DrawPath(t, currentPath.path);
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AnyColor = true;
            cd.ShowDialog();
            this.myColor = cd.Color;
        }

        #endregion
    }   
}
 
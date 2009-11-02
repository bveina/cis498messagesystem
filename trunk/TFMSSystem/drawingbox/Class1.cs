using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace DrawingBox
{
    public partial class DrawingBox3 : UserControl
    {

        private int BigSize = 10;
        private int MedSize = 4;
        private int SmlSize = 1;
        private List<PathData> myPaths;
        private bool shouldPaint;
        private Point lastLocation;
        private PathData currentPath;
        private Color myColor;
        private int myWidth;
        private bool eraseMode, drawingMode;

        public DrawingBox3()
        {
            InitializeComponent();
        }

        public Color lineColor
        {
            get { return myColor; }
            set { myColor = value; }
        }
        public int lineWidth
        {
            get { return myWidth; }
            set { myWidth = value; }
        }
        public List<PathData> lines
        {
            get { return myPaths; }
        }
        public void Clear()
        {
            myPaths.Clear();
            currentPath = new PathData();
            this.Invalidate();
        }
        public Bitmap getImage()
        {
            Bitmap temp = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(temp);
            foreach (PathData pd in myPaths)
                g.DrawPath(new Pen(pd.pathColor, pd.pathWidth), pd.path);
            return temp;
        }

        private void DrawingBox3_Load(object sender, EventArgs e)
        {
            currentPath = new PathData(new GraphicsPath(), myColor, myWidth);
            myPaths = new List<PathData>();
            drawingMode = true;
            eraseMode = false;
            cmdDraw.Checked = true;
            cmdSizeTiny.Checked = true;
            this.myColor = Color.Black;
            this.myWidth = 1;

        }
        private void DrawingBox_Resize(object sender, EventArgs e)
        {
            DrawingBox3 mySender = (DrawingBox3)sender;
            mySender.Invalidate();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            cmdErase.Checked = false;
            drawingMode = true;
            eraseMode = false;
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            cmdDraw.Checked = false;
            drawingMode = false;
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
                if ((index = getPathIndexFromPoint(myPaths, e.Location)) != -1)
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
            if (this.BackgroundImageLayout == ImageLayout.None)
                e.Graphics.DrawImage(this.BackgroundImage, 0, 0);
            else if (this.BackgroundImageLayout == ImageLayout.Stretch)
                e.Graphics.DrawImage(this.BackgroundImage, temp);
            else if (this.BackgroundImageLayout == ImageLayout.Center)
                e.Graphics.DrawImageUnscaled(this.BackgroundImage, this.Width - this.BackgroundImage.Width / 2, this.Height - this.BackgroundImage.Height / 2);

            foreach (PathData pd in myPaths)
            {
                if (pd.path == null) continue;
                Pen p = new Pen(pd.pathColor, pd.pathWidth);
                e.Graphics.DrawPath(p, pd.path);
                p.Dispose();
            }
            if (currentPath != null && currentPath.path != null)
            {
                Pen t = new Pen(currentPath.pathColor, currentPath.pathWidth);
                e.Graphics.DrawPath(t, currentPath.path);
            }
        }

        public bool isPointOnPath(PathData pd, Point p)
        {
            PointF a, b;
            Rectangle c;
            if (pd.path.PointCount == 0) return false;
            for (int i = 0; i < pd.path.PathPoints.Length - 1; i++)
            {
                a = pd.path.PathPoints[i];
                b = pd.path.PathPoints[i + 1];
                c = new Rectangle(
                     (int)Math.Min(a.X, b.X),
                     (int)Math.Min(a.Y, b.Y),
                     (int)Math.Abs(a.X - b.X) + pd.pathWidth + 1,
                     (int)Math.Abs(a.Y - b.Y) + pd.pathWidth + 1);
                if (c.Contains(p)) return true;
            }

            return false;
        }
        public int getPathIndexFromPoint(List<PathData> myPaths, Point p)
        {
            foreach (PathData pd in myPaths)
            {
                if (isPointOnPath(pd, p)) return myPaths.IndexOf(pd);
            }
            return -1;
        }



    }
}

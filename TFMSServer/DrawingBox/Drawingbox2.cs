using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace DrawingBox
{
    public partial class DrawingBox2 : UserControl
    {
        private List<PathData> myPaths;
        private bool shouldPaint;
        private Point lastLocation;
        private PathData currentPath;
        private Color myColor;
        private int myWidth;

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

        public DrawingBox2()
        {
            InitializeComponent();
        }

        private void DrawingBox_MouseDown(object sender, MouseEventArgs e)
        {
            shouldPaint = true;
            currentPath=new PathData(new GraphicsPath(),myColor,myWidth);
            lastLocation = e.Location;
            Cursor.Clip = this.RectangleToScreen(this.ClientRectangle);
        }

        private void DrawingBox_MouseMove(object sender, MouseEventArgs e)
        {
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

        
        private void DrawingBox_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Clip = Screen.PrimaryScreen.Bounds;
            shouldPaint = false;
            if (currentPath != null)
            {
                myPaths.Add(currentPath);
                this.Invalidate();
            }
        }

        private void DrawingBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            if (this.BackgroundImageLayout == ImageLayout.None)
                e.Graphics.DrawImage(this.BackgroundImage, 0, 0);
            else if (this.BackgroundImageLayout == ImageLayout.Stretch)
                e.Graphics.DrawImage(this.BackgroundImage, this.ClientRectangle);

            foreach (PathData pd in myPaths)
            {
                //if (pd.path == null) continue;
                Pen p = new Pen(pd.pathColor, pd.pathWidth);
                e.Graphics.DrawPath(p, pd.path);
                p.Dispose();
            }
            if (currentPath.path != null)
            {
                Pen t = new Pen(currentPath.pathColor, currentPath.pathWidth);
                e.Graphics.DrawPath(t, currentPath.path);
            }
        }

        public void Clear()
        {
            myPaths.Clear();
            currentPath = new PathData();
            this.Invalidate();
        }

        private void DrawingBox_Load(object sender, EventArgs e)
        {
            currentPath = new PathData(new GraphicsPath(), myColor, myWidth);
            myPaths = new List<PathData>();
        }

        public Bitmap getImage()
        {
            Bitmap temp=new Bitmap(this.Width,this.Height);
            Graphics g = Graphics.FromImage(temp);
            foreach (PathData pd in myPaths)
                g.DrawPath(new Pen(pd.pathColor, pd.pathWidth), pd.path);
            return temp;
        }

        private void DrawingBox_Resize(object sender, EventArgs e)
        {
            DrawingBox2 mySender = (DrawingBox2)sender;
            mySender.Invalidate();
        }
    }
    
}
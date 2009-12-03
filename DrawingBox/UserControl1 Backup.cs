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

    public partial class DrawingBox : UserControl
    {
        private Color myColor = Color.Black;
        private int myWidth = 10;
        protected Bitmap myBitmap;
        private bool shouldPaint;

        private Point lastLocation;
        private GraphicsPath myPath;


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

        public DrawingBox()
        {
            InitializeComponent();
        }

        private void DrawingBox_MouseDown(object sender, MouseEventArgs e)
        {
            shouldPaint = true;
            myPath = new System.Drawing.Drawing2D.GraphicsPath();
            lastLocation = e.Location;
            Cursor.Clip = this.RectangleToScreen(this.ClientRectangle);
        }

        private void DrawingBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!shouldPaint) return;
            if (lastLocation == e.Location) return;
            myPath.AddLine(lastLocation, e.Location);
            //Todo: invalidate less...
            this.Invalidate(new Rectangle(Math.Min(lastLocation.X, e.X) - myWidth / 2,
                                          Math.Min(lastLocation.Y, e.Y) - myWidth / 2,
                                          Math.Abs(lastLocation.X - e.X) + myWidth,
                                          Math.Abs(lastLocation.Y - e.Y) + myWidth));
            lastLocation = e.Location;
        }

        private void DrawingBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(myBitmap, 0, 0);
            if (myPath != null)
            {
                Pen myPen = new Pen(myColor, myWidth);
                e.Graphics.DrawPath(myPen, myPath);
                myPen.Dispose();
            }
        }

        private void DrawingBox_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor.Clip = Screen.PrimaryScreen.Bounds;
            shouldPaint = false;
            if (myPath != null)
            {
                Pen myPen = new Pen(myColor, myWidth);
                Graphics a = Graphics.FromImage(myBitmap);
                a.DrawPath(myPen, myPath);
                myPen.Dispose();
                a.Dispose();
            }
        }
        public void Clear()
        {
            myBitmap.Dispose();
            myBitmap = new Bitmap(this.Width, this.Height);
            if (myPath != null) myPath.Dispose();
            myPath = new GraphicsPath();
            this.Invalidate();
        }

        private void DrawingBox_Load(object sender, EventArgs e)
        {
            myBitmap = new Bitmap(this.Width, this.Height);
        }

        public Bitmap getImage()
        {
            return myBitmap;
        }

        private void DrawingBox_Resize(object sender, EventArgs e)
        {
            DrawingBox mySender = (DrawingBox)sender;
            if (myBitmap == null)
            {
                mySender.myBitmap = new Bitmap(mySender.Width, mySender.Height);
            }
            else if (mySender.Size.Height > mySender.myBitmap.Size.Height || mySender.Size.Width > mySender.myBitmap.Size.Width)
            {
                Bitmap temp = mySender.myBitmap;
                mySender.myBitmap = new Bitmap(mySender.Size.Width, mySender.Size.Height);
                Graphics a = Graphics.FromImage(mySender.myBitmap);
                a.DrawImage(temp, 0, 0);
                a.Dispose();
                temp.Dispose();
            }
            mySender.Invalidate();
        }
    }
}
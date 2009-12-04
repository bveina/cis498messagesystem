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
    public partial class vectorBox : UserControl
    {
        List<PathData> myPaths;
        Rectangle myPathBounds;

        public Rectangle pathBounds
        {
            get { return myPathBounds; }
            set { myPathBounds = value; }
        }
        public int pathWidth
        {
            get { return myPathBounds.Width; }
            set { myPathBounds.Width = value; }
        }
        public int pathHeight
        {
            get { return myPathBounds.Height; }
            set { myPathBounds.Height = value; }
        }
        public List<PathData> Paths
        {
            get { return myPaths; }
            set { myPaths = value; }
        }

        public PointF[] pathCorners
        {
            get
            {
                int w = pathBounds.Width;
                int h = pathBounds.Height;
                PointF[] newRect ={new PointF(0,0),new PointF(w,0),
                                   new PointF(0,h),new PointF(w,h)};
                return newRect;
            }            
        }
        public PointF[] clientCorners
        {
            get
            {
                int w = this.ClientRectangle.Width;
                int h = this.ClientRectangle.Height;
                PointF[] newRect ={new PointF(0,0),new PointF(w,0),
                                   new PointF(0,h),new PointF(w,h)};
                return newRect;
            }
        }

        public vectorBox()
        {
            InitializeComponent();
            Paths = null;
            pathBounds = new Rectangle();
        }

        private void vectorBox_Paint(object sender, PaintEventArgs e)
        {
            Rectangle temp = this.ClientRectangle;
            //e.Graphics.Clear(this.BackColor);
            
            if (this.BackgroundImage != null)
            {
                if (this.BackgroundImageLayout == ImageLayout.None)
                    e.Graphics.DrawImage(this.BackgroundImage, 0, 0);
                else if (this.BackgroundImageLayout == ImageLayout.Stretch)
                    e.Graphics.DrawImage(this.BackgroundImage, temp);
                else if (this.BackgroundImageLayout == ImageLayout.Center)
                    e.Graphics.DrawImageUnscaled(this.BackgroundImage, this.Width - this.BackgroundImage.Width / 2, this.Height - this.BackgroundImage.Height / 2);
            }
            if (Paths==null) return; // dont bother trying to draw stuff if you have no paths
            foreach (PathData pd in Paths)
            {
                if (pd.path == null) continue;
                Pen p = new Pen(pd.pathColor, pd.pathWidth);
                GraphicsPath tempPath = (GraphicsPath)pd.path.Clone();
                tempPath.Warp(clientCorners,pathBounds);
                e.Graphics.DrawPath(p, tempPath);
                p.Dispose();
            }
        }

        private void vectorBox_Load(object sender, EventArgs e)
        {

        }
    }
}

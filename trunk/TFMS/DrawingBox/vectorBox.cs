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
    /// <summary>
    /// The VectorBox class is a control that allows the user to create a set of paths within a path bounds region
    /// The set of path represents the message that the user wants to send, and the path bounds represents the area in which the message is drawn
    /// </summary>
    public partial class VectorBox : UserControl
    {
        #region VectorBox class variables

        /// <summary>
        /// The set of paths that the user draws to represent the user's message
        /// </summary>
        private List<PathData> myPaths;

        /// <summary>
        /// The messages bounds are contained within a rectangular region
        /// </summary>
        private Rectangle myPathBounds;

        #endregion

        #region VectorBox constructors

        /// <summary>
        /// Default constructor: creates a new VectorBox component, sets the initial path set to null and creates a new path bounds region
        /// </summary>
        public VectorBox()
        {
            InitializeComponent();
            myPaths = null;
            pathBounds = new Rectangle();
        }

        #endregion

        #region VectorBox "get" and "set" overridden methods

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

        #endregion

        #region VectorBox event actions

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

        #endregion
    }
}

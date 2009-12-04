using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace DrawingBox
{
    [Serializable]
    public class PathData :ISerializable
    {
        /* 
        public GraphicsPath path
        {
            get
            {
                if (pathPoints.Length == 0) return new GraphicsPath();
                else return new GraphicsPath(pathPoints, pathTypes, myFill);
            }
        }        


        private PointF[] pathPoints { get; set; }
        private byte[] pathTypes { get; set; }
        private FillMode myFill { get; set; }
         */
        public Color pathColor;
        public int pathWidth;
        
        public GraphicsPath path;        
        public Pen myPen
        {
            get { return new Pen(pathColor, pathWidth); }
        }
        public PathData() 
            :this(new GraphicsPath(),Color.Black,1) 
        { }
        public PathData(GraphicsPath p, Color c, int w)
        {
            path = p;
            /*pathPoints = new PointF[p.PointCount];
            pathTypes = new byte[p.PointCount];
            p.PathData.Points.CopyTo(pathPoints, 0);
            p.PathData.Types.CopyTo(pathTypes, 0);
            myFill = p.FillMode;
             */
            pathColor = c;
            pathWidth = w;

        }
        #region ISerializable Members
        public PathData(SerializationInfo info, StreamingContext context)
        {
            pathColor = (Color)info.GetValue("pathColor", typeof(Color));
            pathWidth = info.GetInt32("pathWidth");
            PointF[] pathPoints= (PointF[]) info.GetValue("pathPoints",typeof(PointF[]));
            byte[] pathTypes = (byte[]) info.GetValue("pathTypes",typeof(byte[]));
            FillMode myFill = (FillMode)info.GetValue("fillMode", typeof(FillMode));
            if (pathPoints.Length < 1 || pathTypes.Length < 1) path = new GraphicsPath();
            else path = new GraphicsPath(pathPoints, pathTypes);
        }
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("pathColor", pathColor);
            info.AddValue("pathWidth", pathWidth);
            info.AddValue("pathPoints", path.PathData.Points);
            info.AddValue("pathTypes", path.PathData.Types);
            info.AddValue("fillMode", path.FillMode);
        }

        #endregion

    }
}

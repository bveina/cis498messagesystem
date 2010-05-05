using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Runtime.Serialization;
using System.Security.Permissions;


namespace DrawingBox
{
    /// <summary>
    /// When a TFMS user creates a message in the GUI, each individual line on the canvas represents
    /// a PathData object. A single PathData object consists of a set of lines, a color, and the 
    /// width of the path.
    /// 
    /// NOTE: though they are similar, this is different from System.Drawing.Drawing2D.PathData
    /// </summary>
    [Serializable]
    public class PathData : ISerializable
    {
        #region PathData class variables

        /// <summary>
        /// A GraphicsPath object is an array of 2D points representing the line itself
        /// </summary>
        public GraphicsPath path;

        /// <summary>
        /// Represents the color of the GraphsPath
        /// </summary>
        public Color pathColor;

        /// <summary>
        /// Represents the width of the path, in pixels
        /// </summary>
        public int pathWidth;

        #endregion

        #region PathData constructors

        /// <summary>
        /// Default constructor: if no parameters are given, create a new GraphicsPath object, set
        /// the default color to black, and default line width to 1 pixel
        /// </summary>
        public PathData()
            : this(new GraphicsPath(), Color.Black, 1) { }

        /// <summary>
        /// A GraphicsPath object (a line drawn by the user), the user-selected color, and the 
        /// user-selected line width
        /// </summary>
        /// <param name="p">the GraphicsPath object representing a line on the canvas</param>
        /// <param name="c">the Color object representing the color of the line</param>
        /// <param name="w">the width of the line</param>
        public PathData(GraphicsPath p, Color c, int w)
        {
            path = p;
            pathColor = c;
            pathWidth = w;
        }

        /// <summary>
        /// Use the serialized data to extract the GraphicsPath object, the path color, and the path width
        /// </summary>
        /// <param name="info">the data needed to serialize/deserialize the PathData object</param>
        /// <param name="context">the source and destination of the serializaiton stream</param>
        public PathData(SerializationInfo info, StreamingContext context)
        {
            // extract the path color
            pathColor = (Color)info.GetValue("pathColor", typeof(Color));
            
            // extract the path width
            pathWidth = info.GetInt32("pathWidth");
            
            // extract the array of points
            PointF[] pathPoints = (PointF[])info.GetValue("pathPoints", typeof(PointF[]));

            // extract the path type
            byte[] pathTypes = (byte[])info.GetValue("pathTypes", typeof(byte[]));
            
            // if the incoming message has no data, create a new path instead of trying to read null data
            // NOTE: a client is not allowed to send a blank message, but a single-pixel message may be
            // serialized to contain 0 paths (in other words, a blank message can still be received even
            // though the user cannot send a blank message)
            if (pathPoints.Length < 1)
                path = new GraphicsPath();
            else 
                path = new GraphicsPath(pathPoints, pathTypes);
        }

        #endregion

        #region PathData ISerializable methods

        /// <summary>
        /// The GetObjectData method is required by the ISerializable interface to deserialize
        /// the PathData object
        /// </summary>
        /// <param name="info">the data needed to serialize or deserialize an object</param>
        /// <param name="context">the source and destination of a serialized stream</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("pathColor", pathColor);
            info.AddValue("pathWidth", pathWidth);
            info.AddValue("pathPoints", path.PathData.Points);
            info.AddValue("pathTypes", path.PathData.Types);
        }

        #endregion
    }
}
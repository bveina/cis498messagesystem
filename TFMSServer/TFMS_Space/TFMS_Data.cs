using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

/************************************************************************
 * Portions of this code are adapted from the following document:       *
 * Title: "A Chat Application using Asynchronous TCP Sockets"           *
 * Author: Hitesh Sharma                                                *
 * URL: http://www.codeproject.com/KB/IP/ChatAsynchTCPSockets.aspx      *
 ************************************************************************/

namespace TFMS_Space
{
    /// <summary>
    /// The data structure by which TFMS_Server and TFMS_Client relay messages to each other
    /// </summary>
    [Serializable]
    public class TFMS_Data
    {
        #region TFMS_Data class variables

        /// <summary>
        /// String corresponding to the name of the user who sent the message
        /// </summary>
        public string strName;

        /// <summary>
        /// The actual data in the message
        /// </summary>
        public string strMessage;

        /// <summary>
        /// The command field that can be interpreted by the client or receiver and varies depeding on need
        /// </summary>
        public TFMS_Command cmdCommand;

        /// <summary>
        /// If a user looks at a message in the GUI, it is flagged as acknowledged
        /// </summary>
        public bool acknowledged;

        /// <summary>
        /// Time stamp to show when the message was sent
        /// </summary>
        public DateTime timeStamp;

        #endregion

        #region TFMS_Data constructors

        /// <summary>
        /// Default constructor: creates a new TFMS_Data object with null fields
        /// </summary>
        public TFMS_Data()
            : this(TFMS_Command.Null, null, null) { }

        /// <summary>
        /// Creates a new TFMS_Data object that contains a command, a message, and a user name, all in separate fields
        /// encapsulates all the data about a message into one object
        /// </summary>
        /// <param name="cmd">the message command</param>
        /// <param name="msg">the data portion</param>
        /// <param name="name">the user name of the sender</param>
        public TFMS_Data(TFMS_Command cmd, string msg, string name)
        {
            cmdCommand = cmd;
            strMessage = msg;
            strName = name;
            acknowledged = false;
            timeStamp = DateTime.Now;
        }

        /// <summary>
        /// Creates a new TFMS_Data object by interpreting an array of bytes to fill the fields
        /// </summary>
        /// <param name="data">the byte array to be deserialized</param>
        public TFMS_Data(byte[] data)
        {
            TFMS_Data temp;

            try
            {
                // read the xml data into a generic object
                using (MemoryStream ms = new MemoryStream(data)) // using actually converts into a try/catch that disposes of ms at the end
                {
                    XmlSerializer xser = new XmlSerializer(typeof(TFMS_Data));
                    string tempstr = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

                    // cast the data into a TFMS_Data object
                    temp = (TFMS_Data)xser.Deserialize(ms);
                }

            }
            catch (Exception)
            {
                this.clone(new TFMS_Data(TFMS_Command.Null, "", "oops"));
                return;
            }

            // create a new TFMS_Data object by copying the temporary TFMS_Data object
            this.clone(temp);
        }

        #endregion

        #region TFMS_Data public methods

        /// <summary>
        ///  Convert the TFMS_Data object into an array of bytes 
        /// </summary>
        /// <returns>the byte array representing the XML serialized data object</returns>
        public byte[] ToByte()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xser = new XmlSerializer(typeof(TFMS_Data));
                xser.Serialize(ms, this);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// Received messages are displayed green for unacknowledged (unread) messages and red for acknowledged (read) messages
        /// </summary>
        public Brush dispColor
        {
            get
            {
                if (!acknowledged)
                    return Brushes.Red;
                else
                    return Brushes.Green;
            }
        }

        #endregion

        #region TFMS_Data private methods

        /// <summary>
        /// Performs a shallow copy of the specified TFMS_Data object
        /// </summary>
        /// <param name="data">the TFMS_Data object that you want to duplicate</param>
        private void clone(TFMS_Data data)
        {
            if (data == null) return;
            this.acknowledged = data.acknowledged;
            this.cmdCommand = data.cmdCommand;
            this.strMessage = data.strMessage;
            this.strName = data.strName;
            this.timeStamp = data.timeStamp;
        }

        #endregion
    }
}

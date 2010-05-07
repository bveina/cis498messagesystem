using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ComplexChatRoom
{
    static class TFMS_Client_Main
    {
        /// <summary>
        /// The TFMS_Client_Main class provides one of the Main methods for running the TFMS program
        /// TFMS_Client_Main starts a TFMS_GUI, which allows a user to log in to the system
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TFMS_GUI());
        }
    }
}

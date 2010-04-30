using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ComplexChatRoom
{
    static class TFMS_Client_Main
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TFMS_GUI());
        }
    }
}

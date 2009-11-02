using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TFMS_Server
{
    public delegate void timerTick(object sender, EventArgs e);

    class Functions
    {
        public static void startTimer(ref Timer timer, int rate)
        {
            timer = new Timer();
            timer.Interval = rate;
            timer.Start();
        }

        public static void stopTimer(ref Timer timer)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}

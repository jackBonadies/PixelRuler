using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.Common
{
    public class MouseWheelAccumulator
    {
        private int thresholdAmount = 30;
        private int mouseWheelAccumAmount = 0;
        public double Zoom(int zoomAmount)
        {
            if ((mouseWheelAccumAmount > 0 && zoomAmount < 0) ||
               (mouseWheelAccumAmount < 0 && zoomAmount > 0))
            {
                // if now going opposite way, reset
                mouseWheelAccumAmount = 0;
            }
            mouseWheelAccumAmount += zoomAmount;

            if (mouseWheelAccumAmount >= thresholdAmount)
            {
                mouseWheelAccumAmount %= thresholdAmount;
            }
            else if (mouseWheelAccumAmount <= -thresholdAmount)
            {
                mouseWheelAccumAmount %= thresholdAmount;
            }
            else
            {
                // did not pass threshold
                return 0;
            }
            return zoomAmount > 0 ? 2 : .5;
        }
    }
}

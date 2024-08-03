using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.Common
{
    public static class MathUtils
    {
        public static double NextPowerOf2(double val)
        {
            var power = Math.Log2(val);
            power = Math.Floor(power);
            return Math.Pow(2, (int)power + 1);
        }

        public static double PrevPowerOf2(double val)
        {
            var power = Math.Log2(val) - 1;
            power = Math.Ceiling(power);
            return Math.Pow(2, (int)power);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.Common
{
    public class IntBucket
    {
        double accumValue = 0;
        public void Add(double val)
        {
            accumValue += val;
        }
        public int GetValue()
        {
            var intVal = (int)accumValue;
            if (intVal == 0)
            {
                return 0;
            }
            else
            {
                accumValue = accumValue - intVal;
                return intVal;
            }
        }
    }
}

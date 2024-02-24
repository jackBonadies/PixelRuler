using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRuler.CanvasElements
{
    public interface IOverlayCanvasShape
    {
        public void Clear();
        public void AddToCanvas();
        public void UpdateForCoordinatesChanged();
    }
}

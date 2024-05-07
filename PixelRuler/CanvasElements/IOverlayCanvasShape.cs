namespace PixelRuler.CanvasElements
{
    public interface IOverlayCanvasShape
    {
        public void Clear();
        public void AddToCanvas();
        public void UpdateForCoordinatesChanged();
    }
}

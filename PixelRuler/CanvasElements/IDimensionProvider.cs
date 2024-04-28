namespace PixelRuler
{
    public interface IDimensionProvider
    {
        public int Dim1
        {
            get; set;
        }

        public int Dim2
        {
            get; set;
        }

        public bool Has2Dim
        {
            get; set;
        }
    }
}
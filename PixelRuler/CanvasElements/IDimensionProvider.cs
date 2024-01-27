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

        public int Has2Dim
        {
            get; set;
        }
    }
}
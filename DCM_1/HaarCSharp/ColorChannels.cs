namespace TestHaarCSharp
{
    using System;
    using System.Drawing;

    public abstract class ColorChannels
    {
        protected ColorChannels(int width, int height)
        {
            this.Red = new double[width, height];
            this.Green = new double[width, height];
            this.Blue = new double[width, height];
        }

        public double[,] Blue { get; private set; }

        public double[,] Green { get; private set; }

        public double[,] Red { get; private set; }

        public static ColorChannels CreateColorChannels(bool safe, int width, int height)
        {
            if (safe)
            {
                return new SafeColorChannels(width, height);
            }

            return new UnsafeColorChannels(width, height);
        }

        public abstract void MergeColors(Bitmap bmp);

        public abstract void SeparateColors(Bitmap bmp);

        protected static double Scale(double fromMin, double fromMax, double toMin, double toMax, double x)
        {
            if (Math.Abs(fromMax - fromMin) < .01)
            {
                return 0;
            }

            var value = ((toMax - toMin) * (x - fromMin)) / (fromMax - fromMin) + toMin;
            if (value > toMax)
            {
                value = toMax;
            }

            if (value < toMin)
            {
                value = toMin;
            }

            return value;
        }
    }
}
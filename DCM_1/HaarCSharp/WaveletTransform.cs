namespace TestHaarCSharp
{
    using System;

    public abstract class WaveletTransform
    {
        protected const double S0 = 0.5;

        protected const double S1 = 0.5;

        protected const double W0 = 0.5;

        protected const double W1 = -0.5;

        protected WaveletTransform(int iterations)
        {
            this.Iterations = iterations;
        }

        protected WaveletTransform(int width, int height)
        {
            this.Iterations = GetMaxScale(width, height);
        }

        protected int Iterations { get; private set; }

        public static WaveletTransform CreateTransform(bool forward, int iterations)
        {
            if (forward)
            {
                return new ForwardWaveletTransform(iterations);
            }

            return new InverseWaveletTransform(iterations);
        }

        public static int GetMaxScale(int width, int height)
        {
            return (int)(Math.Log(width < height ? width : height) / Math.Log(2));
        }

        public abstract void Transform(ColorChannels channels);
    }
}
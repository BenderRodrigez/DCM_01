namespace TestHaarCSharp
{
    using System.Collections.Generic;

    public class ForwardWaveletTransform : WaveletTransform
    {
        public ForwardWaveletTransform(int iterations)
            : base(iterations)
        {
        }

        public ForwardWaveletTransform(int width, int height)
            : base(width, height)
        {
        }

        public override void Transform(ColorChannels channels)
        {
            foreach (var color in new[] { channels.Red, channels.Green, channels.Blue })
            {
                Transform(color, this.Iterations);
            }
        }

        private static void Transform(IList<double> data)
        {
            var temp = new double[data.Count];

            var h = data.Count >> 1;
            for (var i = 0; i < h; i++)
            {
                var k = i << 1;
                temp[i] = (data[k] * S0) + (data[k + 1] * S1);
                temp[i + h] = (data[k] * W0) + (data[k + 1] * W1);
            }

            for (var i = 0; i < data.Count; i++)
            {
                data[i] = temp[i];
            }
        }

        private static void Transform(double[,] data, int iterations)
        {
            var rows = data.GetLength(0);
            var cols = data.GetLength(1);

            var row = new double[cols];
            var col = new double[rows];

            for (var k = 0; k < iterations; k++)
            {
                for (var i = 0; i < rows; i++)
                {
                    for (var j = 0; j < row.Length; j++)
                    {
                        row[j] = data[i, j];
                    }

                    Transform(row);

                    for (var j = 0; j < row.Length; j++)
                    {
                        data[i, j] = row[j];
                    }
                }

                for (var j = 0; j < cols; j++)
                {
                    for (var i = 0; i < col.Length; i++)
                    {
                        col[i] = data[i, j];
                    }

                    Transform(col);

                    for (var i = 0; i < col.Length; i++)
                    {
                        data[i, j] = col[i];
                    }
                }
            }
        }
    }
}
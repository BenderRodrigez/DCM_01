namespace TestHaarCSharp
{
    using System.Collections.Generic;

    public class InverseWaveletTransform : WaveletTransform
    {
        public InverseWaveletTransform(int iterations)
            : base(iterations)
        {
        }

        public InverseWaveletTransform(int width, int height)
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
                temp[k] = ((data[i] * S0) + (data[i + h] * W0)) / W0;
                temp[k + 1] = ((data[i] * S1) + (data[i + h] * W1)) / S0;
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

            var col = new double[rows];
            var row = new double[cols];

            for (var l = 0; l < iterations; l++)
            {
                for (var j = 0; j < cols; j++)
                {
                    for (var i = 0; i < row.Length; i++)
                    {
                        col[i] = data[i, j];
                    }

                    Transform(col);

                    for (var i = 0; i < col.Length; i++)
                    {
                        data[i, j] = col[i];
                    }
                }

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
            }
        }
    }
}
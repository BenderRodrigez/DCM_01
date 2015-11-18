using System;
using System.Collections.Generic;
using System.Drawing;

namespace DCM_1
{
    class RLE
    {
        private readonly byte[] _source;
        private readonly List<Tuple<byte,ushort>> _compressedStream;
        private readonly Bitmap _sourceImage;

        public RLE(byte[] data)
        {
            _source = data;
            _compressedStream = new List<Tuple<byte, ushort>>(data.Length);
        }

        public RLE(Bitmap image)
        {
            _sourceImage = image;
        }

        public Tuple<byte, ushort>[] Compress()
        {
            var curentWord = _source[0];
            var runLength = (ushort) 0;
            for (int i = 1; i < _source.Length; i++)
            {
                if (_source[i] == curentWord)
                {
                    runLength++;
                }
                else
                {
                    _compressedStream.Add(new Tuple<byte, ushort>(curentWord, runLength));
                    curentWord = _source[i];
                    runLength = 0;
                }
            }
            _compressedStream.Add(new Tuple<byte, ushort>(curentWord, runLength));
            return _compressedStream.ToArray();
        }

        public Tuple<Color, ushort>[] CompressImage()
        {
            var compressed = new List<Tuple<Color, ushort>>(_sourceImage.Width*_sourceImage.Height);
            var colors = new List<Color>(compressed.Capacity);
            for (int i = 0; i < _sourceImage.Width; i++)
            {
                for (int j = 0; j < _sourceImage.Height; j++)
                {
                    colors.Add(_sourceImage.GetPixel(i, j));
                }
            }
            var curentWord = colors[0];
            var runLength = (ushort)0;
            for (int i = 1; i < colors.Count; i++)
            {
                if (colors[i] == curentWord)
                {
                    runLength++;
                }
                else
                {
                    compressed.Add(new Tuple<Color, ushort>(curentWord, runLength));
                    curentWord = colors[i];
                    runLength = 0;
                }
            }
            compressed.Add(new Tuple<Color, ushort>(curentWord, runLength));
            return compressed.ToArray();
        }

        public static Bitmap DecompressImage(IEnumerable<Tuple<Color, ushort>> data, int width, int height)
        {
            var result = new Bitmap(width, height);
            var i = 0;
            var j = 0;
            foreach (var cortage in data)
            {
                for (int k = 0; k <= cortage.Item2; k++)
                {
                    if (j < result.Width)
                    {
                        result.SetPixel(i, j, cortage.Item1);
                        j++;
                    }
                    else
                    {
                        i++;
                        j = 0;
                        result.SetPixel(i, j, cortage.Item1);
                        j++;
                    }
                }
            }
            return result;
        }

        public static byte[] Decompress(IEnumerable<Tuple<byte, ushort>> data)
        {
            var result = new List<byte>();
            foreach (var cortage in data)
            {
                for (var i = 0; i <= cortage.Item2; i++)
                {
                    result.Add(cortage.Item1);
                }
            }
            return result.ToArray();
        }
    }
}

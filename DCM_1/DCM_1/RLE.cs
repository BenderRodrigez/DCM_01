using System;
using System.Collections.Generic;

namespace DCM_1
{
    class RLE
    {
        private readonly byte[] _source;
        private readonly List<Tuple<byte,short>> _compressedStream;

        public RLE(byte[] data)
        {
            _source = data;
            _compressedStream = new List<Tuple<byte, short>>(data.Length);
        }

        public Tuple<byte,short>[] Compress()
        {
            var curentWord = _source[0];
            var runLength = (short)1;
            for (int i = 1; i < _source.Length; i++)
            {
                if (_source[i] == curentWord)
                {
                    runLength++;
                }
                else
                {
                    _compressedStream.Add(new Tuple<byte, short>(curentWord, runLength));
                    curentWord = _source[i];
                    runLength = 1;
                }
            }
            _compressedStream.Add(new Tuple<byte, short>(curentWord, runLength));
            return _compressedStream.ToArray();
        }

        public static byte[] Decompress(IEnumerable<Tuple<byte, short>> data)
        {
            var result = new List<byte>();
            foreach (var cortage in data)
            {
                for (var i = 0; i < cortage.Item2; i++)
                {
                    result.Add(cortage.Item1);
                }
            }
            return result.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;

namespace DCM_1
{
    struct VqImage: ICompressedImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[][] CodeBook { get; set; }
        public Tuple<byte,ushort>[] Image { get; set; }

        public void FromByteArray(byte[] data)
        {
            var int32 = new byte[4];
            Array.Copy(data, int32, 4);
            Width = BitConverter.ToInt32(int32, 0);
            Array.Copy(data, 4, int32, 0, 4);
            Height = BitConverter.ToInt32(int32, 0);
            var cbSize = data[8];
            var cb = new List<byte[]>();
            for (int i = 0; i <= cbSize; i++)
            {
                var codeWord = new byte[3];
                Array.Copy(data, 9 + i*3, codeWord, 0, 3);
                cb.Add(codeWord);
            }
            CodeBook = cb.ToArray();

            var img = new List<Tuple<byte, ushort>>(data.Length - 12 + cbSize*3);
            for (int i = 12 + cbSize*3; i < data.Length - 2; i += 3)
            {
                var single = new byte[2];
                Array.Copy(data, i + 1, single, 0, 2);
                img.Add(new Tuple<byte, ushort>(data[i], BitConverter.ToUInt16(single, 0)));
            }
            Image = img.ToArray();
        }

        public byte[] ToByteArray()
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(Width));
            bytes.AddRange(BitConverter.GetBytes(Height));

            var cbSize = (byte) (CodeBook.Length - 1);
            bytes.Add(cbSize);
            foreach (var b in CodeBook)
            {
                bytes.AddRange(b);
            }

            foreach (var tuple in Image)
            {
                bytes.Add(tuple.Item1);
                bytes.AddRange(BitConverter.GetBytes(tuple.Item2));
            }
            return bytes.ToArray();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;

namespace DCM_1
{
    struct DwtImage: ICompressedImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Tuple<Color, ushort>[] Image { get; set; }

        public void FromByteArray(byte[] data)
        {
//            if((data.Length - 8)%5 > 0)
//                throw new ArgumentException("Массив байт имел неверный размер");

            var int32 = new byte[4];
            Array.Copy(data, int32, 4);
            Width = BitConverter.ToInt32(int32, 0);
            Array.Copy(data, 4, int32, 0, 4);
            Height = BitConverter.ToInt32(int32, 0);
            
            var img = new List<Tuple<Color, ushort>>(data.Length - 8);
            for (int i = 8; i < data.Length - 5; i += 5)
            {
                var single = new byte[2];
                Array.Copy(data, i + 3, single, 0, 2);
                img.Add(new Tuple<Color, ushort>(Color.FromArgb(data[i], data[i + 1], data[i+2]), BitConverter.ToUInt16(single, 0)));
            }
            Image = img.ToArray();
        }

        public byte[] ToByteArray()
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(Width));
            bytes.AddRange(BitConverter.GetBytes(Height));

            foreach (var tuple in Image)
            {
                bytes.AddRange(new[] {tuple.Item1.R, tuple.Item1.G, tuple.Item1.B});
                bytes.AddRange(BitConverter.GetBytes(tuple.Item2));
            }
            return bytes.ToArray();
        }
    }
}
using Huffman.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace Huffman.Bytes
{
    class ByteBuilder
    {
        private readonly List<byte> bytes;
        private long bitFilled;

        public ByteBuilder()
        {
            bytes = new List<byte>();
            bitFilled = 0;
        }

 
        public void Append(BitArray bitArray)
        {
            while (bitArray != null && bitArray.Length > 0)
            {
                if (bitFilled % 8 == 0)
                {
                    bytes.Add(new byte());
                }

                byte b = bytes[bytes.Count - 1];
                var result = AppendBitToByte(bitArray, ref b, (int)bitFilled % 8);
                bytes[bytes.Count - 1] = b;
                bitFilled += result.RestPositionIntoBit - (bitFilled % 8);
                bitArray = result.RestBits;
            }
        }


        public bool IsByteRedy()
        {
            return bitFilled > 8;
        }

        public byte GetByte()
        {
            byte b = bytes[0];
            bytes.RemoveAt(0);
            bitFilled -= 8;
            return b;
        }

        public IEnumerable<byte> GetAllBytes()
        {
            return bytes;
        }
        
        /// <summary>
        /// Convert BitArray into Byte
        /// </summary>
        /// <param name="bitArray">Bit Array which converted Byte</param>
        /// <param name="b">Byte converted by BitArray</param>
        /// <param name="position">position for free bits in byte</param>
        /// <returns> rest bits, rest bits in byte</returns>
        private AppendBitResult AppendBitToByte(BitArray bitArray, ref byte b, int position)
        {
            int bitArrayPointer = 0; 
            while (position < 8 && bitArrayPointer < bitArray.Length)
            {
                SetBit(ref b, position, bitArray[bitArrayPointer]);
                position++;
                bitArrayPointer++;
            }

            AppendBitResult result = new AppendBitResult();

            if (bitArrayPointer < bitArray.Length)
            {
                result.RestBits = bitArray.CopyToNew(bitArrayPointer);
            }
            if (position < 8)
            {
                result.RestPositionIntoBit = position;
            }
            
            return result;
        }


        public static void SetBit(ref byte byteValue, int position, bool bitValue)
        {
            if (bitValue)
            {
                byteValue = (byte)(byteValue | (1 << position));
            }
            else
            {
                byteValue = (byte)(byteValue & ~(1 << position));
            }
        }
    }
}

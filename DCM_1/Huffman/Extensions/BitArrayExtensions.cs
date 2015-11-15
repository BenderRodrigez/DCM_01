using System.Collections;

namespace Huffman.Extensions
{
    static class BitArrayExtensions
    {
        public static BitArray CopyToNew(this BitArray bitArray, int startPosition)
        {
            int count = bitArray.Count - startPosition;
            BitArray newBitArray = new BitArray(count);
            for (int i = 0; i < newBitArray.Length; i++)
            {
                newBitArray[i] = bitArray[i + startPosition];
            }
            return newBitArray;
        }
    }
}

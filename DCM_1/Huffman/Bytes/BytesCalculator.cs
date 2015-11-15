using System.Collections.Generic;

namespace Huffman.Bytes
{
    static class BytesCalculator
    {
        public static Dictionary<byte, long> Calculate(IEnumerable<byte> bytes)
        {
            return Calculate(bytes,  InitializeDictionary());
        }

        public static Dictionary<byte, long> Calculate(IEnumerable<byte> bytes, Dictionary<byte, long> dictionary)
        {
            foreach (byte b in bytes)
            {
                dictionary[b]++;
            }

            return dictionary;
        }

        private static Dictionary<byte, long> InitializeDictionary()
        {
            var dictionary = new Dictionary<byte, long>();
            byte b = byte.MinValue;
            do 
            {
              dictionary.Add(b, 0);
            } while(b++ < byte.MaxValue);
            
            return dictionary;
        }
    }
}
